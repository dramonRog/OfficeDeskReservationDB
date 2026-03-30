# Office Desk Reservation System - Database Project

This is an administrative and testing application (.NET MAUI + EF Core), designed to demonstrate, optimize, and compare the performance of relational and non-relational databases. The data schema is based on the architecture of an office desk reservation system (hot-desking), modeling the physical and organizational structure of an office.

The application is not an end-user program for employees, but a **developer tool** used for managing the database schema, mass generating test data, transforming models, and running benchmarks.

## 🛠 Technologies Used

* **Platform / GUI:** C# 12, .NET 8, .NET MAUI (Multi-platform App UI)
* **Databases:** * Microsoft SQL Server (Main relational database, 11 connected tables)
* **MongoDB** (Non-relational NoSQL document database)
* **ORM:** Entity Framework Core
* **Auxiliary tools:** Bogus (data generation), UraniumUI / FontAwesome (icons)

## ✨ Actual Program Features

The application features a dedicated **Database Admin** panel that performs the following live operations:

1. **Structure Management (EF Core):** The ability to create an empty database and tables from scratch (`EnsureCreated`) or completely delete it (`EnsureDeleted`) straight from the application interface.
2. **Advanced Data Generator (Bogus):** A script that populates 11 relational tables (Locations, Rooms, Desks, Equipment, Users, Reservations, Issues) with tens of thousands of records on the fly. The script automatically matches foreign keys and prevents data collisions (e.g., overlapping reservations).
3. **SQL $\rightarrow$ NoSQL Migration:** A module capable of fetching highly normalized data from SQL Server (multiple `JOIN`/`Include` operations), transforming it into flat, denormalized JSON documents, and saving it in a local MongoDB instance.
4. **Performance Benchmarks:** A built-in system that runs complex queries (Deep Fetch, Search by Email, Mass Update) and directly compares their execution times (in milliseconds) between SQL Server and MongoDB.
5. **Data Transfer (JSON):** A serialization and deserialization mechanism that allows for easy export of the entire relational database state to a text file and its restoration.

## 🚀 How to Run

### Requirements
* Visual Studio 2022 (with the `.NET MAUI` workload installed).
* Local **SQL Server** instance (e.g., `localhost` or `SQLEXPRESS`). The Connection String can be found in the `OfficeDeskReservation.Core/Data/AppDbContext.cs` file.
* A locally running **MongoDB** service (default port `27017` - required only for NoSQL tests).

### Running the application
1. Clone the repository: `git clone https://github.com/dramonRog/OfficeDeskReservationDB.git`
2. Open the `OfficeDeskReservationDB.slnx` file in Visual Studio.
3. Set the `OfficeDeskReservationSystemUI` project as the startup project.
4. **Important (Windows):** To prevent the application from being blocked by Windows security (AppLocker policies for packaged apps), expand the launch options on the top bar of Visual Studio and select the **`OfficeDeskReservationSystemUI (Unpackaged)`** profile.
5. Run the program (F5).
6. Go to the **Database Admin** tab $\rightarrow$ click *Create Empty Database* $\rightarrow$ generate test data using the *Data Generator*.
