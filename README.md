# ValiStore - E-commerce Website for Luggage

<p align="center">
  <img src="https://img.shields.io/badge/.NET_Framework_4.8.1-512BD4?logo=dotnet&logoColor=white&style=flat" />
  <img src="https://img.shields.io/badge/ASP.NET_MVC-512BD4?logo=dotnet&logoColor=white&style=flat" />
  <img src="https://img.shields.io/badge/SQL_Server-CC2927?logo=microsoftsqlserver&logoColor=white&style=flat" />
  <img src="https://img.shields.io/badge/SignalR-5E5E5E?logo=signalr&logoColor=white&style=flat" />
  <img src="https://img.shields.io/badge/Bootstrap-7952B3?logo=bootstrap&logoColor=white&style=flat" />
  <img src="https://img.shields.io/badge/jQuery-0769AD?logo=jquery&logoColor=white&style=flat" />
</p>

## Introduction

ValiStore is an online e-commerce platform dedicated to luggage retail. The system is designed around three distinct roles: **Admin**, **Customer**, and **Shipper**, each with tailored workflows for management, purchasing, and delivery operations.

> **Note:** The project was previously deployed on AWS for real-world usage, but the hosting environment is no longer maintained and the current setup is local-only.

## Tech Stack

| Layer | Technologies |
| --- | --- |
| Framework/Runtime | ASP.NET MVC 5 (.NET Framework 4.8.1) |
| ORM / Database | Entity Framework 6, SQL Server |
| Real-time & Middleware | SignalR, OWIN |
| Frontend | Bootstrap 5, jQuery, jQuery Validation |

## ERD

<img src="https://github.com/user-attachments/assets/2f354c61-72e8-484c-9d73-4cd4527037aa" alt="ValiStore ERD" width="100%" />

## Getting Started

1. Clone the repository:
   ```bash
   git clone https://github.com/Khanguyen2005/ValiStore.git
   cd ValiStore
   ```
2. Restore NuGet packages:
   - In Visual Studio: right-click the solution and choose **Restore NuGet Packages**.
   - Or run `nuget restore` from the solution directory if NuGet CLI is available.
3. Configure the `ConnectionString` in `Web.config` to point to your local SQL Server instance.
4. Initialize the database:
   - If using Code First, run `Update-Database` in the Package Manager Console.
   - If a SQL script is provided, execute it against your local database.
5. Start the project with Visual Studio using **IIS Express**.

## Local Configuration / Secrets

- Real credentials are not committed to the repository.
- Copy the example files and fill in your local values:
  - `Web.AppSettings.Secrets.example.config` -> `Web.AppSettings.Secrets.config`
  - `Web.ConnectionStrings.Secrets.example.config` -> `Web.ConnectionStrings.Secrets.config`
- Update VNPay sandbox keys, PayPal sandbox keys, and the database connection string in the copied files.
- Do not commit real credentials. The `*.Secrets.config` files are ignored by Git.
- The app expects these secret files at runtime; if they are missing, payment and database configuration will fail.

## Key Features

### Customer

- Shopping cart and end-to-end checkout flow.
- Online payments via **VNPay** and **PayPal**.
- Real-time chat with Shipper.

### Admin

- Manage products, categories, brands, and banners.
- Manage operational status.

### Shipper

- Receive assigned orders.
- Update delivery and order status.
- Real-time chat with Customer.

## Contributors

- Nguyễn Đàm Khá
- Đinh Lê Thảo Vy
- Trần Phan Khải
