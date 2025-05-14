# ebProgAssignment
For Programming 3A PROG7311 2025 FT BCAD0701 VCKNDW Term1 GR01 assignment
# eBProgAssignment

An ASP.NET Core MVC application for managing agricultural products with role-based access control and chatbot-driven filtering.

## Table of Contents

* [Description](#description)
* [Features](#features)
* [Technologies Used](#technologies-used)
* [Prerequisites](#prerequisites)
* [Installation](#installation)
* [Configuration](#configuration)
* [Database Setup](#database-setup)
* [Running the Application](#running-the-application)
* [Usage](#usage)
* [Project Structure](#project-structure)
* [Contributing](#contributing)
* [License](#license)
* [Contact](#contact)

## Description

eBProgAssignment is a simple web application/prototype for AgriConnect that allows farmers and employees to list, view, and manage agricultural products. Users can filter products through an integrated Dialogflow chatbot and perform CRUD operations based on their role.

## Features

* **Role-Based Access**: Farmers can create, update, and delete their own products; Employees can view and delete any product and create new users
* **Product Listing**: Display products in a responsive card layout.
* **Chatbot Filtering**: Integrate Dialogflow Messenger for natural language filtering (`equals`, `less than`, `greater than`, and `contains`).
* **Image Support**: Upload or link product images.
* **Confirmation Prompts**: JavaScript confirmation for delete operations.

## Technologies Used

* ASP.NET Core MVC
* Entity Framework Core (Code First)
* SQL Server
* Dialogflow Messenger
* Razor Pages & Bootstrap 5
* JSON Serialization (System.Text.Json)

## Prerequisites

* [.NET 6 SDK](https://dotnet.microsoft.com/download)
* SQL Server (Express or higher)
* A Dialogflow CX or ES agent
* (Optional) Firebase for Authentication

## Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/ST10375204/ebProgAssignment.git
   cd ebProgAssignment
   ```
2. Restore NuGet packages:

   ```bash
   dotnet restore
   ```
3. Update configuration settings (see below).

## Configuration

1. **Connection String**: In `appsettings.json`, located in the api, set your SQL Server connection string:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=AssignmentDb;Trusted_Connection=True;"
     }
   }
   ```
2. **Dialogflow Agent**: In the Razor view that embeds Dialogflow Messenger, replace the `agent-id` attribute with agent’s ID.

## Database Setup

Run the provided SQL script in `provided in the word doc for Eb` (or manually using SSMS) to create the schema and seed initial data.

```sql
CREATE DATABASE AssignmentDb;
GO
USE AssignmentDb;
GO
-- Users and Items table creation scripts...
```

## Running the Application

Execute from the command line:

```bash
dotnet build
dotnet run
```

The application will be available at `https://localhost:5001`.

## Usage

1. **Browse Products**: Visit the Display Products page to see all products.
2. **Filter via Chatbot**: Use the chat widget to type queries like "price greater than 50" or "name contains Jam".
3. **CRUD Operations**:

   * **Farmers**: Create/Update/Delete their own products.
   * **Employees**: Delete any product.

## Project Structure

```
/Controllers      - MVC controllers, Auth for authentication, Db for databse operations
/Models           - EF Core entity models
/Views            - Razor views and layouts
/wwwroot          - Static assets (CSS, JS, images)
/DatabaseScripts  - SQL schema and seed data
```

## Contributing

1. Fork the repository
2. Create a new branch (`git checkout -b feature/my-feature`)
3. Commit your changes (`git commit -m 'Add my feature'`)
4. Push to the branch (`git push origin feature/my-feature`)
5. Open a Pull Request

## License

This project is open-source and available under the [MIT License](LICENSE).

## Contact

For questions or feedback, please open an issue or contact me at deveshgokul45@gmail.com.
