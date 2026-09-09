use axum::{
    extract::{Path, State},
    http::{HeaderMap, StatusCode},
    response::IntoResponse,
    routing::{delete, get, post, put},
    Json, Router,
};
use chrono::{Datelike, IsoWeek, NaiveDate};
use serde::{Deserialize, Serialize};
use sqlx::PgPool;
use std::{net::SocketAddr, sync::Arc};
use tower_governor::{governor::GovernorConfigBuilder, GovernorLayer};

#[derive(Clone)]
struct App {
    pool: PgPool,
    jwt_secret: String,
}

#[derive(Deserialize)]
struct Login {
    email: String,
    password: String,
}

#[derive(Deserialize)]
struct Register {
    email: String,
    password: String,
    #[serde(rename = "confirmPassword", alias = "confirm_password")]
    confirm_password: Option<String>,
}

#[derive(Serialize, Deserialize)]
struct Claims {
    sub: i32,
    exp: usize,
}

#[derive(Deserialize)]
struct StatsRequest {
    period: String,
    metric: String,
    #[serde(rename = "exerciseId")]
    exercise_id: Option<i32>,
}

#[derive(Serialize)]
struct Point {
    date: String,
    value: f64,
}

#[derive(Serialize)]
struct Summary {
    current: Option<f64>,
    best: Option<f64>,
    change: String,
    trend: String,
}

#[derive(Deserialize)]
struct ReorderItem {
    id: i32,
    position: i32,
}

fn user_id(headers: &HeaderMap, secret: &str) -> Option<i32> {
    let a = headers.get("authorization")?.to_str().ok()?;
    let t = a.strip_prefix("Bearer ")?;
    jsonwebtoken::decode::<Claims>(
        t,
        &jsonwebtoken::DecodingKey::from_secret(secret.as_bytes()),
        &jsonwebtoken::Validation::default(),
    )
    .ok()
    .map(|d| d.claims.sub)
}

fn need_auth(headers: &HeaderMap, s: &App) -> Result<i32, axum::response::Response> {
    user_id(headers, &s.jwt_secret).ok_or_else(|| {
        (
            StatusCode::UNAUTHORIZED,
            Json(serde_json::json!({"message": "unauthorized"})),
        )
            .into_response()
    })
}

#[tokio::main]
async fn main() {
    dotenvy::dotenv().ok();
    tracing_subscriber::fmt::init();
    let url = std::env::var("DATABASE_URL").expect("DATABASE_URL");
    let secret = std::env::var("Jwt__Secret")
        .or(std::env::var("JWT_SECRET"))
        .expect("Jwt__Secret or JWT_SECRET must be set - refusing to run with dev key");
    let pool = PgPool::connect(&url).await.unwrap();
    let st = Arc::new(App {
        pool,
        jwt_secret: secret,
    });

    let governor_conf = Arc::new(
        GovernorConfigBuilder::default()
            .per_second(1)
            .burst_size(5)
            .finish()
            .unwrap(),
    );
    let auth_routes = Router::new()
        .route("/api/Auth/login", post(login))
        .route("/api/Auth/register", post(register))
        .layer(GovernorLayer {
            config: governor_conf,
        });

    let cors = tower_http::cors::CorsLayer::new()
        .allow_origin(["https://gym.abuyunus.cc".parse().unwrap()])
        .allow_methods([
            axum::http::Method::GET,
            axum::http::Method::POST,
            axum::http::Method::PUT,
            axum::http::Method::DELETE,
            axum::http::Method::OPTIONS,
        ])
        .allow_headers([
            axum::http::header::CONTENT_TYPE,
            axum::http::header::AUTHORIZATION,
        ]);

    let app = Router::new()
        .route(
            "/api/health",
            get(|| async { Json(serde_json::json!({"status": "ok"})) }),
        )
        .merge(auth_routes)
        .route("/api/Workouts", get(w_list).post(w_create))
        .route("/api/Workouts/:id", get(w_get).put(w_upd).delete(w_del))
        .route("/api/Exercises", get(e_list).post(e_create))
        .route("/api/Exercises/:id", get(e_get).put(e_upd).delete(e_del))
        .route("/api/Presets", get(p_list).post(p_create))
        .route("/api/Presets/:id", get(p_get).put(p_upd).delete(p_del))
        .route("/api/Dashboard", get(d_list).post(d_create))
        .route("/api/Dashboard/:id", put(d_upd).delete(d_del))
        .route("/api/Dashboard/reorder", put(d_reorder))
        .route("/api/Stats/chart-data", post(stats))
        .route("/api/User/settings", get(u_get).put(u_put))
        .layer(cors)
        .with_state(st);
    let addr: SocketAddr = SocketAddr::from((
        [0, 0, 0, 0],
        std::env::var("PORT")
            .ok()
            .and_then(|p| p.parse().ok())
            .unwrap_or(8000),
    ));
    axum::serve(
        tokio::net::TcpListener::bind(addr).await.unwrap(),
        app.into_make_service_with_connect_info::<SocketAddr>(),
    )
    .await
    .unwrap();
}

async fn login(State(s): State<Arc<App>>, Json(b): Json<Login>) -> impl IntoResponse {
    let r: Option<(i32, String)> =
        sqlx::query_as("SELECT \"Id\",\"PasswordHash\" FROM \"AspNetUsers\" WHERE \"Email\"=$1")
            .bind(&b.email)
            .fetch_optional(&s.pool)
            .await
            .unwrap_or_else(|e| {
                tracing::error!("login db failed: {e}");
                None
            });
    if let Some((id, h)) = r {
        use argon2::{Argon2, PasswordHash, PasswordVerifier};
        if let Ok(p) = PasswordHash::new(&h) {
            if Argon2::default()
                .verify_password(b.password.as_bytes(), &p)
                .is_ok()
            {
                let exp = (chrono::Utc::now() + chrono::Duration::hours(12)).timestamp() as usize;
                let t = jsonwebtoken::encode(
                    &jsonwebtoken::Header::default(),
                    &Claims { sub: id, exp },
                    &jsonwebtoken::EncodingKey::from_secret(s.jwt_secret.as_bytes()),
                )
                .unwrap();
                return (
                    StatusCode::OK,
                    Json(serde_json::json!({"token":t,"userId":id})),
                )
                    .into_response();
            }
        }
    }
    (
        StatusCode::BAD_REQUEST,
        Json(serde_json::json!({"message": "invalid"})),
    )
        .into_response()
}

async fn register(State(s): State<Arc<App>>, Json(b): Json<Register>) -> impl IntoResponse {
    if !b.email.contains('@') || b.email.len() <= 5 {
        return (
            StatusCode::BAD_REQUEST,
            Json(serde_json::json!({"message": "Invalid email"})),
        )
            .into_response();
    }
    if b.password.len() < 8 {
        return (
            StatusCode::BAD_REQUEST,
            Json(serde_json::json!({"message": "Password must be at least 8 characters"})),
        )
            .into_response();
    }
    if let Some(cp) = &b.confirm_password {
        if cp != &b.password {
            return (
                StatusCode::BAD_REQUEST,
                Json(serde_json::json!({"message": "Passwords do not match"})),
            )
                .into_response();
        }
    }
    let exists: Option<(i32,)> =
        sqlx::query_as("SELECT \"Id\" FROM \"AspNetUsers\" WHERE \"Email\"=$1")
            .bind(&b.email)
            .fetch_optional(&s.pool)
            .await
            .unwrap_or_else(|e| {
                tracing::error!("register exists check failed: {e}");
                None
            });
    if exists.is_some() {
        return (
            StatusCode::BAD_REQUEST,
            Json(serde_json::json!({"message": "Email already registered"})),
        )
            .into_response();
    }
    use argon2::{
        password_hash::{rand_core::OsRng, SaltString},
        Argon2, PasswordHasher,
    };
    let salt = SaltString::generate(&mut OsRng);
    let h = match Argon2::default().hash_password(b.password.as_bytes(), &salt) {
        Ok(v) => v.to_string(),
        Err(e) => {
            tracing::error!("register hash failed: {e}");
            return (
                StatusCode::INTERNAL_SERVER_ERROR,
                Json(serde_json::json!({"message": "hash failed"})),
            )
                .into_response();
        }
    };
    let r: Option<(i32,)> = sqlx::query_as("INSERT INTO \"AspNetUsers\" (\"UserName\",\"NormalizedUserName\",\"Email\",\"NormalizedEmail\",\"EmailConfirmed\",\"PasswordHash\",\"SecurityStamp\",\"ConcurrencyStamp\",\"PhoneNumberConfirmed\",\"TwoFactorEnabled\",\"LockoutEnabled\",\"AccessFailedCount\") VALUES ($1,$1,$1,$1,false,$2,gen_random_uuid()::text,gen_random_uuid()::text,false,false,true,0) RETURNING \"Id\"")
        .bind(&b.email).bind(&h).fetch_optional(&s.pool).await
        .unwrap_or_else(|e| { tracing::error!("register insert failed: {e}"); None });
    match r {
        Some((id,)) => {
            let exp = (chrono::Utc::now() + chrono::Duration::hours(12)).timestamp() as usize;
            let t = jsonwebtoken::encode(
                &jsonwebtoken::Header::default(),
                &Claims { sub: id, exp },
                &jsonwebtoken::EncodingKey::from_secret(s.jwt_secret.as_bytes()),
            )
            .unwrap();
            (
                StatusCode::OK,
                Json(serde_json::json!({"token":t,"userId":id})),
            )
                .into_response()
        }
        None => (
            StatusCode::BAD_REQUEST,
            Json(serde_json::json!({"message": "exists"})),
        )
            .into_response(),
    }
}

async fn rows(pool: &PgPool, q: &str, uid: i32) -> serde_json::Value {
    sqlx::query_scalar::<_, serde_json::Value>(q)
        .bind(uid)
        .fetch_one(pool)
        .await
        .ok()
        .and_then(|v| if v.is_null() { None } else { Some(v) })
        .unwrap_or(serde_json::json!([]))
}

async fn w_list(h: HeaderMap, State(s): State<Arc<App>>) -> impl IntoResponse {
    match need_auth(&h, &s) {
        Ok(u) => Json(rows(&s.pool,"SELECT json_agg(row_to_json(t)) FROM (SELECT * FROM \"Workouts\" WHERE \"UserId\"=$1 ORDER BY \"Date\" DESC,\"Id\") t",u).await).into_response(),
        Err(e) => e,
    }
}

async fn w_get(h: HeaderMap, State(s): State<Arc<App>>, Path(id): Path<i32>) -> impl IntoResponse {
    match need_auth(&h, &s) {
        Ok(u) => {
            let v: Option<serde_json::Value> = sqlx::query_scalar("SELECT row_to_json(t) FROM (SELECT * FROM \"Workouts\" WHERE \"Id\"=$1 AND \"UserId\"=$2) t").bind(id).bind(u).fetch_optional(&s.pool).await.unwrap_or_else(|e| { tracing::error!("w_get failed: {e}"); None });
            match v {
                Some(x) if !x.is_null() => Json(x).into_response(),
                _ => (StatusCode::NOT_FOUND, Json(serde_json::json!({}))).into_response(),
            }
        }
        Err(e) => e,
    }
}

async fn w_create(
    h: HeaderMap,
    State(s): State<Arc<App>>,
    Json(b): Json<serde_json::Value>,
) -> impl IntoResponse {
    match need_auth(&h, &s) {
        Ok(u) => {
            let r: Option<(i32,)> = sqlx::query_as("INSERT INTO \"Workouts\" (\"UserId\",\"Date\",\"Notes\",\"BodyWeight\") VALUES ($1,$2,$3,$4) RETURNING \"Id\"").bind(u).bind(b["date"].as_str().unwrap_or("now()")).bind(b["notes"].as_str()).bind(b["bodyWeight"].as_f64()).fetch_optional(&s.pool).await.unwrap_or_else(|e| { tracing::error!("w_create failed: {e}"); None });
            match r {
                Some((id,)) => {
                    (StatusCode::CREATED, Json(serde_json::json!({"id":id}))).into_response()
                }
                None => (StatusCode::BAD_REQUEST, Json(serde_json::json!({}))).into_response(),
            }
        }
        Err(e) => e,
    }
}

async fn w_upd(
    h: HeaderMap,
    State(s): State<Arc<App>>,
    Path(id): Path<i32>,
    Json(b): Json<serde_json::Value>,
) -> impl IntoResponse {
    match need_auth(&h, &s) {
        Ok(u) => {
            let r = sqlx::query("UPDATE \"Workouts\" SET \"Date\"=COALESCE($1,\"Date\"),\"Notes\"=$2,\"BodyWeight\"=$3 WHERE \"Id\"=$4 AND \"UserId\"=$5").bind(b["date"].as_str()).bind(b["notes"].as_str()).bind(b["bodyWeight"].as_f64()).bind(id).bind(u).execute(&s.pool).await;
            match r {
                Ok(v) if v.rows_affected() > 0 => (StatusCode::OK, Json(b)).into_response(),
                Ok(_) => (StatusCode::NOT_FOUND, Json(serde_json::json!({}))).into_response(),
                Err(e) => {
                    tracing::error!("w_upd failed: {e}");
                    (StatusCode::BAD_REQUEST, Json(serde_json::json!({}))).into_response()
                }
            }
        }
        Err(e) => e,
    }
}

async fn w_del(h: HeaderMap, State(s): State<Arc<App>>, Path(id): Path<i32>) -> impl IntoResponse {
    match need_auth(&h, &s) {
        Ok(u) => {
            let r = sqlx::query("DELETE FROM \"Workouts\" WHERE \"Id\"=$1 AND \"UserId\"=$2")
                .bind(id)
                .bind(u)
                .execute(&s.pool)
                .await;
            match r {
                Ok(v) if v.rows_affected() > 0 => StatusCode::NO_CONTENT.into_response(),
                Ok(_) => (StatusCode::NOT_FOUND, Json(serde_json::json!({}))).into_response(),
                Err(e) => {
                    tracing::error!("w_del failed: {e}");
                    (StatusCode::BAD_REQUEST, Json(serde_json::json!({}))).into_response()
                }
            }
        }
        Err(e) => e,
    }
}

async fn e_list(h: HeaderMap, State(s): State<Arc<App>>) -> impl IntoResponse {
    match need_auth(&h, &s) {
        Ok(u) => Json(rows(&s.pool,"SELECT json_agg(row_to_json(t)) FROM (SELECT * FROM \"Exercises\" WHERE \"UserId\"=$1 OR \"UserId\" IS NULL OR \"IsDefault\"=true ORDER BY \"Id\") t",u).await).into_response(),
        Err(e) => e,
    }
}

async fn e_get(h: HeaderMap, State(s): State<Arc<App>>, Path(id): Path<i32>) -> impl IntoResponse {
    match need_auth(&h, &s) {
        Ok(u) => {
            let v: Option<serde_json::Value> = sqlx::query_scalar("SELECT row_to_json(t) FROM (SELECT * FROM \"Exercises\" WHERE \"Id\"=$1 AND (\"UserId\"=$2 OR \"UserId\" IS NULL OR \"IsDefault\"=true)) t").bind(id).bind(u).fetch_optional(&s.pool).await.unwrap_or_else(|e| { tracing::error!("e_get failed: {e}"); None });
            match v {
                Some(x) if !x.is_null() => Json(x).into_response(),
                _ => (StatusCode::NOT_FOUND, Json(serde_json::json!({}))).into_response(),
            }
        }
        Err(e) => e,
    }
}

async fn e_create(
    h: HeaderMap,
    State(s): State<Arc<App>>,
    Json(b): Json<serde_json::Value>,
) -> impl IntoResponse {
    match need_auth(&h, &s) {
        Ok(u) => {
            let ex: Option<(i32,)> = sqlx::query_as("SELECT \"Id\" FROM \"Exercises\" WHERE \"Name\"=$1 AND (\"UserId\"=$2 OR \"IsDefault\"=true)").bind(b["name"].as_str().unwrap_or("")).bind(u).fetch_optional(&s.pool).await.unwrap_or_else(|e| { tracing::error!("e_create dup check failed: {e}"); None });
            if ex.is_some() {
                return (
                    StatusCode::BAD_REQUEST,
                    Json(serde_json::json!({"message":"Exercise name already exists"})),
                )
                    .into_response();
            }
            let r: Option<(i32,)> = sqlx::query_as("INSERT INTO \"Exercises\" (\"Name\",\"MuscleGroup\",\"IsDuration\",\"UserId\",\"IsDefault\") VALUES ($1,$2,$3,$4,false) RETURNING \"Id\"").bind(b["name"].as_str().unwrap_or("")).bind(b["muscleGroup"].as_str().unwrap_or("")).bind(b["isDuration"].as_bool().unwrap_or(false)).bind(u).fetch_optional(&s.pool).await.unwrap_or_else(|e| { tracing::error!("e_create insert failed: {e}"); None });
            match r {
                Some((id,)) => {
                    (StatusCode::CREATED, Json(serde_json::json!({"id":id}))).into_response()
                }
                None => (StatusCode::BAD_REQUEST, Json(serde_json::json!({}))).into_response(),
            }
        }
        Err(e) => e,
    }
}

async fn e_upd(
    h: HeaderMap,
    State(s): State<Arc<App>>,
    Path(id): Path<i32>,
    Json(b): Json<serde_json::Value>,
) -> impl IntoResponse {
    match need_auth(&h, &s) {
        Ok(u) => {
            let r = sqlx::query("UPDATE \"Exercises\" SET \"Name\"=$1,\"MuscleGroup\"=$2 WHERE \"Id\"=$3 AND (\"UserId\"=$4 OR \"UserId\" IS NULL) AND \"IsDefault\"=false").bind(b["name"].as_str()).bind(b["muscleGroup"].as_str()).bind(id).bind(u).execute(&s.pool).await;
            match r {
                Ok(v) if v.rows_affected() > 0 => (StatusCode::OK, Json(b)).into_response(),
                Ok(_) => (StatusCode::NOT_FOUND, Json(serde_json::json!({}))).into_response(),
                Err(e) => {
                    tracing::error!("e_upd failed: {e}");
                    (StatusCode::BAD_REQUEST, Json(serde_json::json!({}))).into_response()
                }
            }
        }
        Err(e) => e,
    }
}

async fn e_del(h: HeaderMap, State(s): State<Arc<App>>, Path(id): Path<i32>) -> impl IntoResponse {
    match need_auth(&h, &s) {
        Ok(u) => {
            let r = sqlx::query("DELETE FROM \"Exercises\" WHERE \"Id\"=$1 AND \"UserId\"=$2 AND \"IsDefault\"=false").bind(id).bind(u).execute(&s.pool).await;
            match r {
                Ok(v) if v.rows_affected() > 0 => StatusCode::NO_CONTENT.into_response(),
                Ok(_) => (StatusCode::NOT_FOUND, Json(serde_json::json!({}))).into_response(),
                Err(e) => {
                    tracing::error!("e_del failed: {e}");
                    (StatusCode::BAD_REQUEST, Json(serde_json::json!({}))).into_response()
                }
            }
        }
        Err(e) => e,
    }
}

async fn p_list(h: HeaderMap, State(s): State<Arc<App>>) -> impl IntoResponse {
    match need_auth(&h, &s) {
        Ok(u) => Json(rows(&s.pool,"SELECT json_agg(row_to_json(t)) FROM (SELECT * FROM \"Presets\" WHERE \"UserId\"=$1 ORDER BY \"Id\") t",u).await).into_response(),
        Err(e) => e,
    }
}

async fn p_get(h: HeaderMap, State(s): State<Arc<App>>, Path(id): Path<i32>) -> impl IntoResponse {
    match need_auth(&h, &s) {
        Ok(u) => {
            let v: Option<serde_json::Value> = sqlx::query_scalar("SELECT row_to_json(t) FROM (SELECT * FROM \"Presets\" WHERE \"Id\"=$1 AND \"UserId\"=$2) t").bind(id).bind(u).fetch_optional(&s.pool).await.unwrap_or_else(|e| { tracing::error!("p_get failed: {e}"); None });
            match v {
                Some(x) if !x.is_null() => Json(x).into_response(),
                _ => (StatusCode::NOT_FOUND, Json(serde_json::json!({}))).into_response(),
            }
        }
        Err(e) => e,
    }
}

async fn p_create(
    h: HeaderMap,
    State(s): State<Arc<App>>,
    Json(b): Json<serde_json::Value>,
) -> impl IntoResponse {
    match need_auth(&h, &s) {
        Ok(u) => {
            let r: Option<(i32,)> = sqlx::query_as(
                "INSERT INTO \"Presets\" (\"Name\",\"UserId\") VALUES ($1,$2) RETURNING \"Id\"",
            )
            .bind(b["name"].as_str().unwrap_or(""))
            .bind(u)
            .fetch_optional(&s.pool)
            .await
            .unwrap_or_else(|e| {
                tracing::error!("p_create failed: {e}");
                None
            });
            match r {
                Some((id,)) => {
                    (StatusCode::CREATED, Json(serde_json::json!({"id":id}))).into_response()
                }
                None => (StatusCode::BAD_REQUEST, Json(serde_json::json!({}))).into_response(),
            }
        }
        Err(e) => e,
    }
}

async fn p_upd(
    h: HeaderMap,
    State(s): State<Arc<App>>,
    Path(id): Path<i32>,
    Json(b): Json<serde_json::Value>,
) -> impl IntoResponse {
    match need_auth(&h, &s) {
        Ok(u) => {
            let r =
                sqlx::query("UPDATE \"Presets\" SET \"Name\"=$1 WHERE \"Id\"=$2 AND \"UserId\"=$3")
                    .bind(b["name"].as_str())
                    .bind(id)
                    .bind(u)
                    .execute(&s.pool)
                    .await;
            match r {
                Ok(v) if v.rows_affected() > 0 => (StatusCode::OK, Json(b)).into_response(),
                Ok(_) => (StatusCode::NOT_FOUND, Json(serde_json::json!({}))).into_response(),
                Err(e) => {
                    tracing::error!("p_upd failed: {e}");
                    (StatusCode::BAD_REQUEST, Json(serde_json::json!({}))).into_response()
                }
            }
        }
        Err(e) => e,
    }
}

async fn p_del(h: HeaderMap, State(s): State<Arc<App>>, Path(id): Path<i32>) -> impl IntoResponse {
    match need_auth(&h, &s) {
        Ok(u) => {
            let r = sqlx::query("DELETE FROM \"Presets\" WHERE \"Id\"=$1 AND \"UserId\"=$2")
                .bind(id)
                .bind(u)
                .execute(&s.pool)
                .await;
            match r {
                Ok(v) if v.rows_affected() > 0 => StatusCode::NO_CONTENT.into_response(),
                Ok(_) => (StatusCode::NOT_FOUND, Json(serde_json::json!({}))).into_response(),
                Err(e) => {
                    tracing::error!("p_del failed: {e}");
                    (StatusCode::BAD_REQUEST, Json(serde_json::json!({}))).into_response()
                }
            }
        }
        Err(e) => e,
    }
}

async fn d_list(h: HeaderMap, State(s): State<Arc<App>>) -> impl IntoResponse {
    match need_auth(&h, &s) {
        Ok(u) => Json(rows(&s.pool,"SELECT json_agg(row_to_json(t)) FROM (SELECT * FROM \"DashboardCharts\" WHERE \"UserId\"=$1 ORDER BY \"Position\") t",u).await).into_response(),
        Err(e) => e,
    }
}

async fn d_create(
    h: HeaderMap,
    State(s): State<Arc<App>>,
    Json(b): Json<serde_json::Value>,
) -> impl IntoResponse {
    match need_auth(&h, &s) {
        Ok(u) => {
            let r: Option<(i32,)> = sqlx::query_as("INSERT INTO \"DashboardCharts\" (\"UserId\",\"Label\",\"Metric\") VALUES ($1,$2,$3) RETURNING \"Id\"").bind(u).bind(b["label"].as_str().unwrap_or("")).bind(b["metric"].as_str().unwrap_or("")).fetch_optional(&s.pool).await.unwrap_or_else(|e| { tracing::error!("d_create failed: {e}"); None });
            match r {
                Some((id,)) => {
                    (StatusCode::CREATED, Json(serde_json::json!({"id":id}))).into_response()
                }
                None => (StatusCode::BAD_REQUEST, Json(serde_json::json!({}))).into_response(),
            }
        }
        Err(e) => e,
    }
}

async fn d_upd(
    h: HeaderMap,
    State(s): State<Arc<App>>,
    Path(id): Path<i32>,
    Json(b): Json<serde_json::Value>,
) -> impl IntoResponse {
    match need_auth(&h, &s) {
        Ok(u) => {
            let r = sqlx::query("UPDATE \"DashboardCharts\" SET \"Label\"=COALESCE($1,\"Label\") WHERE \"Id\"=$2 AND \"UserId\"=$3").bind(b["label"].as_str()).bind(id).bind(u).execute(&s.pool).await;
            match r {
                Ok(v) if v.rows_affected() > 0 => (StatusCode::OK, Json(b)).into_response(),
                Ok(_) => (StatusCode::NOT_FOUND, Json(serde_json::json!({}))).into_response(),
                Err(e) => {
                    tracing::error!("d_upd failed: {e}");
                    (StatusCode::BAD_REQUEST, Json(serde_json::json!({}))).into_response()
                }
            }
        }
        Err(e) => e,
    }
}

async fn d_del(h: HeaderMap, State(s): State<Arc<App>>, Path(id): Path<i32>) -> impl IntoResponse {
    match need_auth(&h, &s) {
        Ok(u) => {
            let r =
                sqlx::query("DELETE FROM \"DashboardCharts\" WHERE \"Id\"=$1 AND \"UserId\"=$2")
                    .bind(id)
                    .bind(u)
                    .execute(&s.pool)
                    .await;
            match r {
                Ok(v) if v.rows_affected() > 0 => StatusCode::NO_CONTENT.into_response(),
                Ok(_) => (StatusCode::NOT_FOUND, Json(serde_json::json!({}))).into_response(),
                Err(e) => {
                    tracing::error!("d_del failed: {e}");
                    (StatusCode::BAD_REQUEST, Json(serde_json::json!({}))).into_response()
                }
            }
        }
        Err(e) => e,
    }
}

async fn d_reorder(
    h: HeaderMap,
    State(s): State<Arc<App>>,
    Json(items): Json<Vec<ReorderItem>>,
) -> impl IntoResponse {
    match need_auth(&h, &s) {
        Ok(u) => {
            let mut tx = match s.pool.begin().await {
                Ok(t) => t,
                Err(e) => {
                    tracing::error!("reorder begin tx failed: {e}");
                    return (
                        StatusCode::INTERNAL_SERVER_ERROR,
                        Json(serde_json::json!({})),
                    )
                        .into_response();
                }
            };
            for item in &items {
                let r = sqlx::query("UPDATE \"DashboardCharts\" SET \"Position\"=$1 WHERE \"Id\"=$2 AND \"UserId\"=$3")
                    .bind(item.position).bind(item.id).bind(u)
                    .execute(&mut *tx).await;
                if let Err(e) = r {
                    tracing::error!("reorder update failed for id {}: {e}", item.id);
                    let _ = tx.rollback().await;
                    return (
                        StatusCode::BAD_REQUEST,
                        Json(serde_json::json!({"message":"reorder failed"})),
                    )
                        .into_response();
                }
            }
            match tx.commit().await {
                Ok(_) => (StatusCode::OK, Json(serde_json::json!([]))).into_response(),
                Err(e) => {
                    tracing::error!("reorder commit failed: {e}");
                    (
                        StatusCode::INTERNAL_SERVER_ERROR,
                        Json(serde_json::json!({})),
                    )
                        .into_response()
                }
            }
        }
        Err(e) => e,
    }
}

fn cutoff_date(period: &str) -> Option<NaiveDate> {
    let now = chrono::Utc::now().naive_utc().date();
    match period {
        "7d" => Some(now - chrono::Duration::days(7)),
        "30d" => Some(now - chrono::Duration::days(30)),
        "90d" => Some(now - chrono::Duration::days(90)),
        "180d" => Some(now - chrono::Duration::days(180)),
        "365d" => Some(now - chrono::Duration::days(365)),
        "all" => None,
        _ => Some(now - chrono::Duration::days(30)),
    }
}

fn compute_summary(points: &[Point]) -> Summary {
    if points.is_empty() {
        return Summary {
            current: None,
            best: None,
            change: "0%".into(),
            trend: "flat".into(),
        };
    }
    let first = points.first().unwrap().value;
    let current = points.last().unwrap().value;
    let best = points.iter().map(|p| p.value).fold(f64::MIN, f64::max);
    let change_pct = if first.abs() > f64::EPSILON {
        (current - first) / first.abs() * 100.0
    } else {
        0.0
    };
    let change_str = format!(
        "{}{:.1}%",
        if change_pct >= 0.0 { "+" } else { "" },
        change_pct
    );
    let trend = if change_pct > 1.0 {
        "up"
    } else if change_pct < -1.0 {
        "down"
    } else {
        "flat"
    };
    Summary {
        current: Some(current),
        best: Some(best),
        change: change_str,
        trend: trend.into(),
    }
}

async fn stats(
    h: HeaderMap,
    State(s): State<Arc<App>>,
    Json(req): Json<StatsRequest>,
) -> impl IntoResponse {
    match need_auth(&h, &s) {
        Ok(u) => {
            let cutoff = cutoff_date(&req.period);
            let points: Vec<Point> = if req.metric == "bodyWeight" {
                let rows: Vec<(NaiveDate, Option<f64>)> = sqlx::query_as("SELECT \"Date\"::date, \"BodyWeight\"::float8 FROM \"Workouts\" WHERE \"UserId\"=$1 AND \"BodyWeight\" IS NOT NULL AND ($2::date IS NULL OR \"Date\" >= $2) ORDER BY \"Date\"").bind(u).bind(cutoff).fetch_all(&s.pool).await.unwrap_or_else(|e| { tracing::error!("stats bodyWeight failed: {e}"); vec![] });
                rows.into_iter()
                    .filter_map(|(d, w)| {
                        w.map(|v| Point {
                            date: d.to_string(),
                            value: v,
                        })
                    })
                    .collect()
            } else {
                let rows: Vec<(NaiveDate, Option<i32>, Option<i32>, Option<f64>, Option<i32>, Option<i32>, i32)> = sqlx::query_as("SELECT w.\"Date\"::date, we.\"Sets\", we.\"Reps\", we.\"Weight\"::float8, we.\"Duration\", we.\"DurationUnit\", we.\"WorkoutId\" FROM \"WorkoutExercises\" we JOIN \"Workouts\" w ON w.\"Id\" = we.\"WorkoutId\" WHERE w.\"UserId\"=$1 AND ($2::date IS NULL OR w.\"Date\" >= $2) AND ($3::int IS NULL OR we.\"ExerciseId\" = $3) ORDER BY w.\"Date\"").bind(u).bind(cutoff).bind(req.exercise_id).fetch_all(&s.pool).await.unwrap_or_else(|e| { tracing::error!("stats query failed: {e}"); vec![] });
                use std::collections::BTreeMap;
                let norm = |v: Option<i32>, unit: Option<i32>| -> f64 {
                    let v = v.unwrap_or(0) as f64;
                    match unit.unwrap_or(0) {
                        1 => v * 60.0,
                        2 => v * 3600.0,
                        _ => v,
                    }
                };
                match req.metric.as_str() {
                    "weight" => {
                        let mut m: BTreeMap<NaiveDate, f64> = BTreeMap::new();
                        for (d, _, _, w, _, _, _) in &rows {
                            if let Some(v) = w {
                                m.entry(*d).and_modify(|e| *e = e.max(*v)).or_insert(*v);
                            }
                        }
                        m.into_iter()
                            .map(|(d, v)| Point {
                                date: d.to_string(),
                                value: v,
                            })
                            .collect()
                    }
                    "volume" => {
                        let mut m: BTreeMap<NaiveDate, f64> = BTreeMap::new();
                        for (d, sets, reps, w, _, _, _) in &rows {
                            let v = (sets.unwrap_or(0) as f64)
                                * (reps.unwrap_or(0) as f64)
                                * w.unwrap_or(0.0);
                            *m.entry(*d).or_insert(0.0) += v;
                        }
                        m.into_iter()
                            .map(|(d, v)| Point {
                                date: d.to_string(),
                                value: v,
                            })
                            .collect()
                    }
                    "duration" => {
                        let mut m: BTreeMap<NaiveDate, f64> = BTreeMap::new();
                        for (d, _, _, _, dur, unit, _) in &rows {
                            *m.entry(*d).or_insert(0.0) += norm(*dur, *unit);
                        }
                        m.into_iter()
                            .map(|(d, v)| Point {
                                date: d.to_string(),
                                value: v,
                            })
                            .collect()
                    }
                    "frequency" => {
                        use std::collections::BTreeSet;
                        let mut m: BTreeMap<(i32, u32), BTreeSet<i32>> = BTreeMap::new();
                        for (d, _, _, _, _, _, wid) in &rows {
                            let iso: IsoWeek = d.iso_week();
                            m.entry((iso.year(), iso.week())).or_default().insert(*wid);
                        }
                        m.into_iter()
                            .map(|((y, wk), ids)| Point {
                                date: format!("{y}-W{wk:02}"),
                                value: ids.len() as f64,
                            })
                            .collect()
                    }
                    _ => vec![],
                }
            };
            let summary = compute_summary(&points);
            (
                StatusCode::OK,
                Json(serde_json::json!({ "points": points, "summary": summary })),
            )
                .into_response()
        }
        Err(e) => e,
    }
}

async fn u_get(h: HeaderMap, State(s): State<Arc<App>>) -> impl IntoResponse {
    match need_auth(&h, &s) {
        Ok(u) => {
            let v: Option<serde_json::Value> = sqlx::query_scalar(
                "SELECT row_to_json(t) FROM (SELECT * FROM \"UserSettings\" WHERE \"UserId\"=$1) t",
            )
            .bind(u)
            .fetch_optional(&s.pool)
            .await
            .unwrap_or_else(|e| {
                tracing::error!("u_get failed: {e}");
                None
            });
            Json(v.unwrap_or(serde_json::json!({}))).into_response()
        }
        Err(e) => e,
    }
}

async fn u_put(
    h: HeaderMap,
    State(s): State<Arc<App>>,
    Json(b): Json<serde_json::Value>,
) -> impl IntoResponse {
    match need_auth(&h, &s) {
        Ok(u) => {
            let _ = sqlx::query(
                "INSERT INTO \"UserSettings\" (\"UserId\") VALUES ($1) ON CONFLICT DO NOTHING",
            )
            .bind(u)
            .execute(&s.pool)
            .await;
            (StatusCode::OK, Json(b)).into_response()
        }
        Err(e) => e,
    }
}
