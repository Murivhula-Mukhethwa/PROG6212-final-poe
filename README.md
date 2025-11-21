## My YOUTUBE VIDEO LINK : https://youtu.be/9Q9X1BG2KnM?si=ezsX4wKhIfpl9yFt
# CMCS Claims System – Part 3

## 🚀 What’s New in This Version

* **User Login Added** – Secure, session-based authentication
* **HR Super User Role** – Full control over users and system management
* **Database Integrated** – Built with EF Core and SQL Server
* **Improved Security** – Role-based permissions throughout the system
* **Reporting Tools** – HR can generate claim and user reports

## 👥 System Roles

| Role            | What They Can Do                            |
| --------------- | ------------------------------------------- |
| **HR**          | Full access, manage users, generate reports |
| **Lecturer**    | Submit claims & view previous submissions   |
| **Coordinator** | Verify lecturer claims                      |
| **Manager**     | Approve verified claims                     |

## 🔐 Default Test Logins

* **HR:** `hr@university.com` / `hr123`
* **Lecturer:** `lecturer@university.com` / `lecturer123`
* **Coordinator:** `coordinator@university.com` / `coordinator123`
* **Manager:** `manager@university.com` / `manager123`

## 🛠️ How to Set Up

1. Run the SQL script provided in the **Database** folder using SSMS
2. Update your **connection string** in `appsettings.json`
3. Start the system with `dotnet run`

## ✨ System Highlights

* Automatic user details filled into claims
* Monthly hour checks (max 180 hours)
* Encrypted file uploads
* Dynamic menus based on user role
* Built-in reporting for HR

## 📁 Project Layout

```
Controllers/    – Login, Admin, HR, Claims
Models/         – Users, Claims, ViewModels
Services/       – Repos, Encryption, Reporting
Data/           – EF Core DbContext
Views/          – Pages for each role
```

## 🔧 Technologies Used

* ASP.NET Core MVC
* EF Core
* SSMS
* Sessions
* Bootstrap 
