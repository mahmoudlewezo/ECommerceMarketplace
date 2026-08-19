# ECommerceMarketplace - Development Handoff

## Current Status

Person 1 — Foundation & Authentication is completed.

---

## Completed

### Project Foundation

- ASP.NET Core MVC project configured.
- SQL Server configured.
- Entity Framework Core configured.
- ApplicationDbContext configured.
- Database created successfully.
- Initial migrations created.

### Identity

ASP.NET Core Identity is configured.

Roles:

- Admin
- Seller
- Customer

### Authentication

Implemented:

- Register
- Login
- Logout
- Role-based authorization

### Seller Workflow

Implemented:

- Customer can request to become a Seller.
- Admin can view Seller requests.
- Admin can approve requests.
- Admin can reject requests.
- Approved users receive Seller role.
- Customer role is removed after approval.

### User Management

Admin can:

- View users.
- Suspend users.
- Activate users.

Suspended users:

- Cannot log in.
- Are rejected during authentication validation.

---

## Admin Account

Development account:

Email:

admin@ecommerce.com

Password:

Admin@123

---

## Database

Database is already created through EF Core migrations.

If the database does not exist on another machine:

```powershell
Update-Database