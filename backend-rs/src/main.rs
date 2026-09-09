use axum::{extract::{Path, State}, http::{HeaderMap, StatusCode}, response::IntoResponse, routing::{delete, get, post, put}, Json, Router};
use serde::{Deserialize, Serialize};
use sqlx::PgPool;
use std::{net::SocketAddr, sync::Arc};

#[derive(Clone)] struct App { pool: PgPool, jwt_secret: String }
#[derive(Deserialize)] struct Login { email: String, password: String }
#[derive(Serialize, Deserialize)] struct Claims { sub: i32, exp: usize }

fn user_id(headers: &HeaderMap, secret: &str) -> Option<i32> {
    let a = headers.get("authorization")?.to_str().ok()?;
    let t = a.strip_prefix("Bearer ")?;
    jsonwebtoken::decode::<Claims>(t, &jsonwebtoken::DecodingKey::from_secret(secret.as_bytes()), &jsonwebtoken::Validation::default()).ok().map(|d| d.claims.sub)
}
fn need_auth(headers: &HeaderMap, s: &App) -> Result<i32, axum::response::Response> {
    user_id(headers, &s.jwt_secret).ok_or_else(|| (StatusCode::UNAUTHORIZED, Json(serde_json::json!({"message":"unauthorized"}))).into_response())
}

#[tokio::main]
async fn main() {
    dotenvy::dotenv().ok(); tracing_subscriber::fmt::init();
    let url = std::env::var("DATABASE_URL").expect("DATABASE_URL");
    let secret = std::env::var("Jwt__Secret").or(std::env::var("JWT_SECRET")).unwrap_or("dev".into());
    let pool = PgPool::connect(&url).await.unwrap();
    let st = Arc::new(App { pool, jwt_secret: secret });
    let app = Router::new()
        .route("/api/health", get(|| async { Json(serde_json::json!({"status":"ok"})) }))
        .route("/api/Auth/login", post(login)).route("/api/Auth/register", post(register))
        .route("/api/Workouts", get(w_list).post(w_create)).route("/api/Workouts/:id", get(w_get).put(w_upd).delete(w_del))
        .route("/api/Exercises", get(e_list).post(e_create)).route("/api/Exercises/:id", get(e_get).put(e_upd).delete(e_del))
        .route("/api/Presets", get(p_list).post(p_create)).route("/api/Presets/:id", get(p_get).put(p_upd).delete(p_del))
        .route("/api/Dashboard", get(d_list).post(d_create)).route("/api/Dashboard/:id", put(d_upd).delete(d_del))
        .route("/api/Dashboard/reorder", put(d_reorder))
        .route("/api/Stats/chart-data", post(stats))
        .route("/api/User/settings", get(u_get).put(u_put))
        .layer(tower_http::cors::CorsLayer::permissive()).with_state(st);
    let addr = SocketAddr::from(([0, 0, 0, 0], std::env::var("PORT").ok().and_then(|p| p.parse().ok()).unwrap_or(8000)));
    axum::serve(tokio::net::TcpListener::bind(addr).await.unwrap(), app).await.unwrap();
}

async fn login(State(s): State<Arc<App>>, Json(b): Json<Login>) -> impl IntoResponse {
    let r: Option<(i32, String)> = sqlx::query_as("SELECT \"Id\",\"PasswordHash\" FROM \"AspNetUsers\" WHERE \"Email\"=$1").bind(&b.email).fetch_optional(&s.pool).await.unwrap_or(None);
    if let Some((id, h)) = r {
        use argon2::{Argon2, PasswordHash, PasswordVerifier};
        if let Ok(p) = PasswordHash::new(&h) { if Argon2::default().verify_password(b.password.as_bytes(), &p).is_ok() {
            let exp = (chrono::Utc::now() + chrono::Duration::hours(12)).timestamp() as usize;
            let t = jsonwebtoken::encode(&jsonwebtoken::Header::default(), &Claims { sub: id, exp }, &jsonwebtoken::EncodingKey::from_secret(s.jwt_secret.as_bytes())).unwrap();
            return (StatusCode::OK, Json(serde_json::json!({"token":t,"userId":id}))).into_response();
        }}
    }
    (StatusCode::BAD_REQUEST, Json(serde_json::json!({"message":"invalid"}))).into_response()
}
async fn register(State(s): State<Arc<App>>, Json(b): Json<Login>) -> impl IntoResponse {
    use argon2::{Argon2, PasswordHasher, password_hash::{SaltString, rand_core::OsRng}};
    let salt = SaltString::generate(&mut OsRng);
    let h = Argon2::default().hash_password(b.password.as_bytes(), &salt).unwrap().to_string();
    let r: Option<(i32,)> = sqlx::query_as("INSERT INTO \"AspNetUsers\" (\"UserName\",\"NormalizedUserName\",\"Email\",\"NormalizedEmail\",\"EmailConfirmed\",\"PasswordHash\",\"SecurityStamp\",\"ConcurrencyStamp\",\"PhoneNumberConfirmed\",\"TwoFactorEnabled\",\"LockoutEnabled\",\"AccessFailedCount\") VALUES ($1,$1,$1,$1,false,$2,gen_random_uuid()::text,gen_random_uuid()::text,false,false,true,0) RETURNING \"Id\"").bind(&b.email).bind(&h).fetch_optional(&s.pool).await.unwrap_or(None);
    match r { Some((id,)) => { let exp = (chrono::Utc::now() + chrono::Duration::hours(12)).timestamp() as usize; let t = jsonwebtoken::encode(&jsonwebtoken::Header::default(), &Claims{sub:id,exp}, &jsonwebtoken::EncodingKey::from_secret(s.jwt_secret.as_bytes())).unwrap(); (StatusCode::OK, Json(serde_json::json!({"token":t,"userId":id}))).into_response() } None => (StatusCode::BAD_REQUEST, Json(serde_json::json!({"message":"exists"}))).into_response() }
}
async fn rows(pool: &PgPool, q: &str, uid: i32) -> serde_json::Value {
    sqlx::query_scalar::<_, serde_json::Value>(q).bind(uid).fetch_one(pool).await.ok().and_then(|v| if v.is_null(){None}else{Some(v)}).unwrap_or(serde_json::json!([]))
}
// Workouts
async fn w_list(h: HeaderMap, State(s): State<Arc<App>>) -> impl IntoResponse { match need_auth(&h,&s){Ok(u)=>Json(rows(&s.pool,"SELECT json_agg(row_to_json(t)) FROM (SELECT * FROM \"Workouts\" WHERE \"UserId\"=$1 ORDER BY \"Date\" DESC,\"Id\") t",u).await).into_response(),Err(e)=>e} }
async fn w_get(h: HeaderMap, State(s): State<Arc<App>>, Path(id): Path<i32>) -> impl IntoResponse { match need_auth(&h,&s){Ok(u)=>{let v: Option<serde_json::Value>=sqlx::query_scalar("SELECT row_to_json(t) FROM (SELECT * FROM \"Workouts\" WHERE \"Id\"=$1 AND \"UserId\"=$2) t").bind(id).bind(u).fetch_optional(&s.pool).await.unwrap_or(None); match v{Some(x) if !x.is_null()=>Json(x).into_response(),_=> (StatusCode::NOT_FOUND,Json(serde_json::json!({}))).into_response()}},Err(e)=>e} }
async fn w_create(h: HeaderMap, State(s): State<Arc<App>>, Json(b): Json<serde_json::Value>) -> impl IntoResponse { match need_auth(&h,&s){Ok(u)=>{let r: Option<(i32,)> = sqlx::query_as("INSERT INTO \"Workouts\" (\"UserId\",\"Date\",\"Notes\",\"BodyWeight\") VALUES ($1,$2,$3,$4) RETURNING \"Id\"").bind(u).bind(b["date"].as_str().unwrap_or("now()")).bind(b["notes"].as_str()).bind(b["bodyWeight"].as_f64()).fetch_optional(&s.pool).await.unwrap_or(None); match r{Some((id,))=> (StatusCode::CREATED,Json(serde_json::json!({"id":id}))).into_response(),None=>(StatusCode::BAD_REQUEST,Json(serde_json::json!({}))).into_response()}},Err(e)=>e} }
async fn w_upd(h: HeaderMap, State(s): State<Arc<App>>, Path(id): Path<i32>, Json(b): Json<serde_json::Value>) -> impl IntoResponse { match need_auth(&h,&s){Ok(u)=>{let r=sqlx::query("UPDATE \"Workouts\" SET \"Date\"=COALESCE($1,\"Date\"),\"Notes\"=$2,\"BodyWeight\"=$3 WHERE \"Id\"=$4 AND \"UserId\"=$5").bind(b["date"].as_str()).bind(b["notes"].as_str()).bind(b["bodyWeight"].as_f64()).bind(id).bind(u).execute(&s.pool).await; match r{Ok(v) if v.rows_affected()>0=>(StatusCode::OK,Json(b)).into_response(),Ok(_)=>(StatusCode::NOT_FOUND,Json(serde_json::json!({}))).into_response(),Err(_)=>(StatusCode::BAD_REQUEST,Json(serde_json::json!({}))).into_response()}},Err(e)=>e} }
async fn w_del(h: HeaderMap, State(s): State<Arc<App>>, Path(id): Path<i32>) -> impl IntoResponse { match need_auth(&h,&s){Ok(u)=>{let r=sqlx::query("DELETE FROM \"Workouts\" WHERE \"Id\"=$1 AND \"UserId\"=$2").bind(id).bind(u).execute(&s.pool).await; match r{Ok(v) if v.rows_affected()>0=>StatusCode::NO_CONTENT.into_response(),Ok(_)=>(StatusCode::NOT_FOUND,Json(serde_json::json!({}))).into_response(),Err(_)=>(StatusCode::BAD_REQUEST,Json(serde_json::json!({}))).into_response()}},Err(e)=>e} }
// Exercises
async fn e_list(h: HeaderMap, State(s): State<Arc<App>>) -> impl IntoResponse { match need_auth(&h,&s){Ok(u)=>Json(rows(&s.pool,"SELECT json_agg(row_to_json(t)) FROM (SELECT * FROM \"Exercises\" WHERE \"UserId\"=$1 OR \"UserId\" IS NULL OR \"IsDefault\"=true ORDER BY \"Id\") t",u).await).into_response(),Err(e)=>e} }
async fn e_get(h: HeaderMap, State(s): State<Arc<App>>, Path(id): Path<i32>) -> impl IntoResponse { match need_auth(&h,&s){Ok(_)=>{let v: Option<serde_json::Value>=sqlx::query_scalar("SELECT row_to_json(t) FROM (SELECT * FROM \"Exercises\" WHERE \"Id\"=$1) t").bind(id).fetch_optional(&s.pool).await.unwrap_or(None); match v{Some(x) if !x.is_null()=>Json(x).into_response(),_=> (StatusCode::NOT_FOUND,Json(serde_json::json!({}))).into_response()}},Err(e)=>e} }
async fn e_create(h: HeaderMap, State(s): State<Arc<App>>, Json(b): Json<serde_json::Value>) -> impl IntoResponse { match need_auth(&h,&s){Ok(u)=>{let ex: Option<(i32,)> = sqlx::query_as("SELECT \"Id\" FROM \"Exercises\" WHERE \"Name\"=$1 AND (\"UserId\"=$2 OR \"IsDefault\"=true)").bind(b["name"].as_str().unwrap_or("")).bind(u).fetch_optional(&s.pool).await.unwrap_or(None); if ex.is_some(){return (StatusCode::BAD_REQUEST,Json(serde_json::json!({"message":"Exercise name already exists"}))).into_response();} let r: Option<(i32,)> = sqlx::query_as("INSERT INTO \"Exercises\" (\"Name\",\"MuscleGroup\",\"IsDuration\",\"UserId\") VALUES ($1,$2,$3,$4) RETURNING \"Id\"").bind(b["name"].as_str().unwrap_or("")).bind(b["muscleGroup"].as_str().unwrap_or("")).bind(b["isDuration"].as_bool().unwrap_or(false)).bind(u).fetch_optional(&s.pool).await.unwrap_or(None); match r{Some((id,))=>(StatusCode::CREATED,Json(serde_json::json!({"id":id}))).into_response(),None=>(StatusCode::BAD_REQUEST,Json(serde_json::json!({}))).into_response()}},Err(e)=>e} }
async fn e_upd(h: HeaderMap, State(s): State<Arc<App>>, Path(id): Path<i32>, Json(b): Json<serde_json::Value>) -> impl IntoResponse { match need_auth(&h,&s){Ok(u)=>{let r=sqlx::query("UPDATE \"Exercises\" SET \"Name\"=$1,\"MuscleGroup\"=$2 WHERE \"Id\"=$3 AND (\"UserId\"=$4 OR \"UserId\" IS NULL) AND \"IsDefault\"=false").bind(b["name"].as_str()).bind(b["muscleGroup"].as_str()).bind(id).bind(u).execute(&s.pool).await; match r{Ok(v) if v.rows_affected()>0=>(StatusCode::OK,Json(b)).into_response(),Ok(_)=>(StatusCode::NOT_FOUND,Json(serde_json::json!({}))).into_response(),Err(_)=>(StatusCode::BAD_REQUEST,Json(serde_json::json!({}))).into_response()}},Err(e)=>e} }
async fn e_del(h: HeaderMap, State(s): State<Arc<App>>, Path(id): Path<i32>) -> impl IntoResponse { match need_auth(&h,&s){Ok(u)=>{let r=sqlx::query("DELETE FROM \"Exercises\" WHERE \"Id\"=$1 AND \"UserId\"=$2 AND \"IsDefault\"=false").bind(id).bind(u).execute(&s.pool).await; match r{Ok(v) if v.rows_affected()>0=>StatusCode::NO_CONTENT.into_response(),Ok(_)=>(StatusCode::NOT_FOUND,Json(serde_json::json!({}))).into_response(),Err(_)=>(StatusCode::BAD_REQUEST,Json(serde_json::json!({}))).into_response()}},Err(e)=>e} }
// Presets
async fn p_list(h: HeaderMap, State(s): State<Arc<App>>) -> impl IntoResponse { match need_auth(&h,&s){Ok(u)=>Json(rows(&s.pool,"SELECT json_agg(row_to_json(t)) FROM (SELECT * FROM \"Presets\" WHERE \"UserId\"=$1 ORDER BY \"Id\") t",u).await).into_response(),Err(e)=>e} }
async fn p_get(h: HeaderMap, State(s): State<Arc<App>>, Path(id): Path<i32>) -> impl IntoResponse { match need_auth(&h,&s){Ok(u)=>{let v: Option<serde_json::Value>=sqlx::query_scalar("SELECT row_to_json(t) FROM (SELECT * FROM \"Presets\" WHERE \"Id\"=$1 AND \"UserId\"=$2) t").bind(id).bind(u).fetch_optional(&s.pool).await.unwrap_or(None); match v{Some(x) if !x.is_null()=>Json(x).into_response(),_=> (StatusCode::NOT_FOUND,Json(serde_json::json!({}))).into_response()}},Err(e)=>e} }
async fn p_create(h: HeaderMap, State(s): State<Arc<App>>, Json(b): Json<serde_json::Value>) -> impl IntoResponse { match need_auth(&h,&s){Ok(u)=>{let r: Option<(i32,)> = sqlx::query_as("INSERT INTO \"Presets\" (\"Name\",\"UserId\") VALUES ($1,$2) RETURNING \"Id\"").bind(b["name"].as_str().unwrap_or("")).bind(u).fetch_optional(&s.pool).await.unwrap_or(None); match r{Some((id,))=>(StatusCode::CREATED,Json(serde_json::json!({"id":id}))).into_response(),None=>(StatusCode::BAD_REQUEST,Json(serde_json::json!({}))).into_response()}},Err(e)=>e} }
async fn p_upd(h: HeaderMap, State(s): State<Arc<App>>, Path(id): Path<i32>, Json(b): Json<serde_json::Value>) -> impl IntoResponse { match need_auth(&h,&s){Ok(u)=>{let r=sqlx::query("UPDATE \"Presets\" SET \"Name\"=$1 WHERE \"Id\"=$2 AND \"UserId\"=$3").bind(b["name"].as_str()).bind(id).bind(u).execute(&s.pool).await; match r{Ok(v) if v.rows_affected()>0=>(StatusCode::OK,Json(b)).into_response(),Ok(_)=>(StatusCode::NOT_FOUND,Json(serde_json::json!({}))).into_response(),Err(_)=>(StatusCode::BAD_REQUEST,Json(serde_json::json!({}))).into_response()}},Err(e)=>e} }
async fn p_del(h: HeaderMap, State(s): State<Arc<App>>, Path(id): Path<i32>) -> impl IntoResponse { match need_auth(&h,&s){Ok(u)=>{let r=sqlx::query("DELETE FROM \"Presets\" WHERE \"Id\"=$1 AND \"UserId\"=$2").bind(id).bind(u).execute(&s.pool).await; match r{Ok(v) if v.rows_affected()>0=>StatusCode::NO_CONTENT.into_response(),Ok(_)=>(StatusCode::NOT_FOUND,Json(serde_json::json!({}))).into_response(),Err(_)=>(StatusCode::BAD_REQUEST,Json(serde_json::json!({}))).into_response()}},Err(e)=>e} }
// Dashboard
async fn d_list(h: HeaderMap, State(s): State<Arc<App>>) -> impl IntoResponse { match need_auth(&h,&s){Ok(u)=>Json(rows(&s.pool,"SELECT json_agg(row_to_json(t)) FROM (SELECT * FROM \"DashboardCharts\" WHERE \"UserId\"=$1 ORDER BY \"Position\") t",u).await).into_response(),Err(e)=>e} }
async fn d_create(h: HeaderMap, State(s): State<Arc<App>>, Json(b): Json<serde_json::Value>) -> impl IntoResponse { match need_auth(&h,&s){Ok(u)=>{let r: Option<(i32,)> = sqlx::query_as("INSERT INTO \"DashboardCharts\" (\"UserId\",\"Label\",\"Metric\") VALUES ($1,$2,$3) RETURNING \"Id\"").bind(u).bind(b["label"].as_str().unwrap_or("")).bind(b["metric"].as_str().unwrap_or("")).fetch_optional(&s.pool).await.unwrap_or(None); match r{Some((id,))=>(StatusCode::CREATED,Json(serde_json::json!({"id":id}))).into_response(),None=>(StatusCode::BAD_REQUEST,Json(serde_json::json!({}))).into_response()}},Err(e)=>e} }
async fn d_upd(h: HeaderMap, State(s): State<Arc<App>>, Path(id): Path<i32>, Json(b): Json<serde_json::Value>) -> impl IntoResponse { match need_auth(&h,&s){Ok(u)=>{let r=sqlx::query("UPDATE \"DashboardCharts\" SET \"Label\"=COALESCE($1,\"Label\") WHERE \"Id\"=$2 AND \"UserId\"=$3").bind(b["label"].as_str()).bind(id).bind(u).execute(&s.pool).await; match r{Ok(v) if v.rows_affected()>0=>(StatusCode::OK,Json(b)).into_response(),Ok(_)=>(StatusCode::NOT_FOUND,Json(serde_json::json!({}))).into_response(),Err(_)=>(StatusCode::BAD_REQUEST,Json(serde_json::json!({}))).into_response()}},Err(e)=>e} }
async fn d_del(h: HeaderMap, State(s): State<Arc<App>>, Path(id): Path<i32>) -> impl IntoResponse { match need_auth(&h,&s){Ok(u)=>{let r=sqlx::query("DELETE FROM \"DashboardCharts\" WHERE \"Id\"=$1 AND \"UserId\"=$2").bind(id).bind(u).execute(&s.pool).await; match r{Ok(v) if v.rows_affected()>0=>StatusCode::NO_CONTENT.into_response(),Ok(_)=>(StatusCode::NOT_FOUND,Json(serde_json::json!({}))).into_response(),Err(_)=>(StatusCode::BAD_REQUEST,Json(serde_json::json!({}))).into_response()}},Err(e)=>e} }
async fn d_reorder(h: HeaderMap, State(s): State<Arc<App>>, Json(_b): Json<serde_json::Value>) -> impl IntoResponse { match need_auth(&h,&s){Ok(_)=>(StatusCode::OK,Json(serde_json::json!([]))).into_response(),Err(e)=>e} }
async fn stats(h: HeaderMap, State(s): State<Arc<App>>, Json(_b): Json<serde_json::Value>) -> impl IntoResponse { match need_auth(&h,&s){Ok(_)=>(StatusCode::OK,Json(serde_json::json!({"points":[],"summary":{}}))).into_response(),Err(e)=>e} }
async fn u_get(h: HeaderMap, State(s): State<Arc<App>>) -> impl IntoResponse { match need_auth(&h,&s){Ok(u)=>{let v: Option<serde_json::Value>=sqlx::query_scalar("SELECT row_to_json(t) FROM (SELECT * FROM \"UserSettings\" WHERE \"UserId\"=$1) t").bind(u).fetch_optional(&s.pool).await.unwrap_or(None); Json(v.unwrap_or(serde_json::json!({}))).into_response()},Err(e)=>e} }
async fn u_put(h: HeaderMap, State(s): State<Arc<App>>, Json(b): Json<serde_json::Value>) -> impl IntoResponse { match need_auth(&h,&s){Ok(u)=>{let _=sqlx::query("INSERT INTO \"UserSettings\" (\"UserId\") VALUES ($1) ON CONFLICT DO NOTHING").bind(u).execute(&s.pool).await; (StatusCode::OK,Json(b)).into_response()},Err(e)=>e} }
