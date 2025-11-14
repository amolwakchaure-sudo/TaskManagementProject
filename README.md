# Task Management Platform (Microservices)

A **production-ready**, **scalable**, and **secure** microservices architecture using **.NET 8**, **MongoDB**, and **Docker**.

---

## Services

| Service            | Port | Purpose |
|--------------------|------|--------|
| `UserService`      | `44394` | User Authentication & Role Management |
| `TaskService`      | `44304` | Task CRUD, SLA, Priority, Assignment |
| `ReportingService` | `5001`  | Summary, SLA Breaches, Tasks by User |
| `MongoDB`          | `27017` | NoSQL Database |

---

## Features Implemented

### UserService
- Full CRUD (Create, Read, Update, Delete)
- `/api/auth/login` → returns stub JWT-like token: `userId_role`
- Roles: **Admin**, **Manager**, **Engineer**
- **Admin-only delete** with proper role check
- Swagger with Bearer token support

### TaskService
- Full CRUD + Delete
- Status workflow: **Open → In Progress → Blocked → Completed**
- **Activity log** on every status change
- Filters: `status`, `assigneeId`, `from`, `to`
- SLA flag: `IsOverdue` when `DueDate < today && Status != Completed`
- Token-based authorization on Update/Delete

### ReportingService
- `GET /api/reports/tasks-by-user` → Tasks per user with status breakdown
- `GET /api/reports/tasks-by-status` → Count by status
- `GET /api/reports/sla-breaches` → Overdue tasks with **DaysOverdue**
- Real-time aggregation from MongoDB
- Token-protected endpoints

---

## Technology Stack
- **.NET 8.0** (Minimal APIs + Controllers)
- **MongoDB** (shared database)
- **Docker** + **docker-compose** (optional)
- **Swagger/OpenAPI** with Bearer Auth
- **Git** with logical commits
- **Visual Studio 2022**

---

## Prerequisites

1. **Docker Desktop** [](https://www.docker.com/products/docker-desktop)
2. **.NET 8 SDK** [](https://dotnet.microsoft.com/download)
3. **Visual Studio 2022** (or VS Code)

---
## How to Run

### Visual Studio (Current Setup - RECOMMENDED)
1. Open `TaskManagementPlatform.sln`
2. Set **Multiple startup projects**
3. Start: **UserService**, **TaskService**, **ReportingService**
4. Press **F5**
---

## Quick Start (Docker)

```bash
# 1. Clone repo
git clone https://github.com/amolwakchaure-sudo/TaskManagementProject.git
cd TaskManagementPlatform

# 2. Start all services
docker-compose up --build
