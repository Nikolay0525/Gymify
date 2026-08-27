# Gymify

Gymify is a gamified workout web application designed to boost motivation and actively engage users in their fitness routines. The platform combines the workout process with gaming mechanics and a strong social component.

## Core Features

* **Gamification and Rewards:** Users create and complete workouts to earn special cases. Various in-game items drop from these cases.
* **Social Interaction:** The platform includes comprehensive user profiles and a friends system.
* **Communication:** Real-time chats are implemented for communication, along with the ability to leave comments on activities.
* **Workout Browser:** A built-in tool to discover new workouts created by other community members and to find new friends with similar interests.

## Technologies

* **Backend:** C#, ASP.NET Core
* **Database:** Entity Framework Core
* **Authentication and Authorization:** ASP.NET Identity
* **Real-time Communication:** SignalR

## Installation and Setup

1. Clone the repository:
   ```bash
   git clone [https://github.com/Nikolay0525/Gymify.git](https://github.com/Nikolay0525/Gymify.git)

2. Navigate to the project folder and open it in your preferred IDE (such as Visual Studio).

3. Update the database connection string and also change email address for sending codes to existing email, in in the appsettings.json.

   <img width="1078" height="200" alt="image" src="https://github.com/user-attachments/assets/9b29b766-cd07-410e-b6f5-7f2dd77e9535" />

4. Apply database migrations:
   * If you are using the **.NET CLI** (Terminal / Command Prompt):
     ```bash
     dotnet ef database update
     ```
   * If you are using Visual Studio's **Package Manager Console**:
     ```powershell
     Update-Database
     ```
5. Run the project through single startup project Gymify.Web.

## How to enter the system

there are two pre-created accounts for user and admin with such credentials:

* User: email:user@gmail.com password:user123!
* Admin: email:admin@gmail.com password:admin123!
