# 🏥 ONLINE BOOKING DOCTOR API

A robust API for an online doctor appointment booking system, facilitating seamless interaction between patients, doctors, and public users.

---

## 🔒 Security and Authentication

This API uses **Bearer Token** authentication for all secure endpoints. You must include a valid JWT in the `Authorization` header of your requests in the format: `Bearer <your_token>`.

### Logout Security Enhancement

We have implemented the **AuthorizeV1Filter** for the logout endpoint (`/api/Account/Logout`). This ensures that only authenticated users can initiate a logout, correctly invalidating the session or token and protecting against unauthorized access to this critical security feature.

### Account Endpoints

| Method | Endpoint | Description | Access |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/Account/Register` | Register a new user (Patient or Doctor). | Public |
| `POST` | `/api/Account/Login` | Authenticate a user and receive a token. | Public |
| `POST` | `/api/Account/Logout` | Invalidate the current user's session/token. | Authenticated |
| `GET` | `/api/Account/me` | Retrieve the current user's details. | Authenticated |
| `GET` | `/api/Account/EmailExists` | Check if an email is already registered. | Public |

---

## 👤 User Roles and Capabilities

The system supports three main user types: **Non-Registered Users (Public)**, **Patients**, and **Doctors**.

### 🌍 Non-Registered User (Public) Capabilities

A user who hasn't logged in or registered can perform essential search and viewing actions:

* **View Departments:** Retrieve a paginated list of medical departments.
    * `GET /api/public/departments`
* **Search Doctors:** Find doctors by name or filter by department.
    * `GET /api/public/doctors`
* **View Doctor Profile:** Get detailed information for a specific doctor.
    * `GET /api/public/doctors/{DoctorId}`
* **View Doctor Schedule:** Check a doctor's available appointment slots.
    * `GET /api/public/doctors/{DoctorId}/schedule`
* **View Doctor Reviews:** See reviews and ratings for a specific doctor.
    * `GET /api/public/doctors/{DoctorId}/reviews`

---

### 💚 Patient Capabilities (Authenticated)

Patients can manage their profile, book appointments, and leave feedback:

| Action | Endpoint | Method |
| :--- | :--- | :--- |
| **Profile Management** | `/api/Patient/me/profile` | `POST`, `GET`, `PUT` |
| **Book Appointment** | `/api/Appointment` | `POST` |
| **View Specific Appointment** | `/api/Appointment/{Appointmentid}` | `GET` |
| **Cancel Appointment** | `/api/Appointment/{Appointmentid}/cancel` | `PUT` |
| **View All Appointments** | `/api/Patient/me/appointments` | `GET` |
| **Submit Review** (for a doctor) | `/api/Review/{doctorId}` | `POST` |
| **Delete Review** (their own) | `/api/Review/{Reviewid}` | `DELETE` |

---

### 👨‍⚕️ Doctor Capabilities (Authenticated)

Doctors can manage their profile, services, schedules, and appointments:

| Action | Endpoint | Method |
| :--- | :--- | :--- |
| **Profile Management** | `/api/Doctor/me/profile` | `POST`, `GET`, `PUT` |
| **Add Clinic Affiliation** | `/api/Doctor/me/clinics/{clinicId}` | `POST` |
| **Create Service** | `/api/services` | `POST` |
| **View Services** | `/api/Doctor/me/services` | `GET` |
| **Update Service** | `/api/services/{ServiceId}` | `PUT` |
| **Delete Service** | `/api/services/{ServiceId}` | `DELETE` |
| **Create Schedule** | `/api/Schedule/schedules` | `POST` |
| **View Schedules** | `/api/Doctor/me/schedules` | `GET` |
| **Update Schedule** | `/api/Schedule/schedules/{ScheduleId}` | `PUT` |
| **Delete Schedule** | `/api/Schedule/schedules/{ScheduleId}` | `DELETE` |
| **View All Appointments** | `/api/Doctor/me/appointments` | `GET` |
| **Confirm Appointment** | `/api/Appointment/{Appointmentid}/confirm` | `PUT` |
| **View Billing Records** | `/api/billing/records` | `GET` |

---
