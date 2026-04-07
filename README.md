# Full-stack ToDo Application

This is the final project for DSS2. It includes a .NET 8 Backend API, a React Frontend, PostgreSQL database, and integrations with Redis and RabbitMQ.

## 🚀 How to run the project (2 ways)

First, clone the repository:
```bash
git clone https://github.com/muzykin/DSS2_FinalProject.git
cd DSS2_FinalProject

1. The Easy Way: Using Docker (Recommended for E2E Tests)
Everything is fully automated. You don't need to install .NET, Node.js, or set any environment variables manually.

Run this single command from the root directory:

docker compose -f Frontend/docker-compose.e2e.yml up --build --exit-code-from cypress

This command will:

Build and start the PostgreSQL database, Redis, and RabbitMQ.
Build and start the .NET Backend (http://localhost:3087).
Build and start the React Frontend (http://localhost:3000).
Automatically apply Entity Framework database migrations.
Run the Cypress E2E tests in a headless container and exit with the test score.
(To view Swagger UI, you can run the same command without --exit-code-from cypress and open http://localhost:3087/swagger in your browser).

2. The Developer Way: Running Locally
If you want to run the project locally for development or manual testing:

Step A: Start the databases You can use the provided e2e compose file just to spin up the databases in the background:

docker compose -f Frontend/docker-compose.e2e.yml up db redis rabbitmq -d

Step B: Start the Backend Open a new terminal:

cd Backend
dotnet run

The API will be running at http://localhost:3087 (Swagger available at /swagger).

Step C: Start the Frontend and Cypress Open another terminal:

cd Frontend
npm install
npm run dev

Open a final terminal to run tests with UI:

cd Frontend
npm run cy:open

📋 Deliverables Note
The required screenshots of the Swagger UI, API requests/responses, and Database tables are included in the Word document report attached to the root of this repository.