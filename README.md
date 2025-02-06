# Employee Management System

A Full Stack Employee Management System built with .NET Core, JWT Authentication, and Blazor for the front end.

## Table of Contents
- [Introduction](#introduction)
- [Features](#features)
- [Technologies](#technologies)
- [Installation](#installation)
- [Usage](#usage)
- [Contributing](#contributing)
- [License](#license)

## Introduction
The Employee Management System is a web application designed to manage employee data efficiently. It includes features such as employee registration, login, and profile management, along with JWT-based authentication for secure access. The front end is built using Blazor, providing a modern, responsive user interface.

## Features
- User Registration and Login
- JWT Authentication
- Employee Profile Management
- Role-Based Access Control
- Responsive User Interface with Blazor
- Secure API Endpoints

## Technologies
- **Backend**: .NET Core
- **Frontend**: Blazor
- **Authentication**: JWT (JSON Web Token)
- **Database**: SQL Server
- **Hosting**: Azure (Optional)

## Installation
### Prerequisites
- [.NET Core SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)

### Backend
1. Clone the repository:
    ```bash
    git clone https://github.com/yourusername/employee-management-system.git
    cd employee-management-system/Backend
    ```

2. Update the `appsettings.json` file with your SQL Server connection string.

3. Run the following command to restore dependencies and build the project:
    ```bash
    dotnet restore
    dotnet build
    ```

4. Apply database migrations:
    ```bash
    dotnet ef database update
    ```

5. Start the backend server:
    ```bash
    dotnet run
    ```

### Frontend
1. Navigate to the frontend project directory:
    ```bash
    cd ../Frontend
    ```

2. Restore dependencies and build the project:
    ```bash
    dotnet restore
    dotnet build
    ```

3. Start the Blazor server:
    ```bash
    dotnet run
    ```

## Usage
1. Open your browser and navigate to `http://localhost:5000` for the backend and `http://localhost:5001` for the frontend.
2. Register a new account or log in with existing credentials.
3. Explore and manage employee data through the user interface.

## Contributing
Contributions are welcome! Please follow these steps:
1. Fork the repository.
2. Create a new branch (`git checkout -b feature/YourFeature`).
3. Commit your changes (`git commit -m 'Add some feature'`).
4. Push to the branch (`git push origin feature/YourFeature`).
5. Open a pull request.

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
