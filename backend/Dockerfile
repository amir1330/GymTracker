FROM rust:1.88-alpine AS builder
RUN apk add --no-cache musl-dev pkgconfig
WORKDIR /app
COPY Cargo.toml ./
COPY src ./src
RUN cargo build --release
FROM gcr.io/distroless/static:nonroot
COPY --from=builder /app/target/release/gymtracker-backend /app
EXPOSE 8000
USER nonroot:nonroot
ENTRYPOINT ["/app"]
