# Smart Hookah IoT System

A comprehensive IoT solution for monitoring and controlling hookah devices, featuring cloud connectivity, real-time data processing, and a web-based management interface.

## System Overview

The Smart Hookah system is designed to connect hookah devices to the cloud, enabling remote monitoring, control, and data analysis. The system consists of multiple components working together to provide a seamless experience for both users and administrators.

### System Architecture

The Smart Hookah system follows a multi-tier architecture:

1. **Device Layer**: Physical hookah devices with embedded sensors and connectivity modules
2. **Communication Layer**: Azure IoT Hub for secure device-to-cloud and cloud-to-device communication
3. **Processing Layer**: Azure WebJobs and background services for message processing and business logic
4. **Data Layer**: SQL Server database for storing device data, user information, and session history
5. **Application Layer**: ASP.NET web application providing the user interface and API endpoints
6. **Client Layer**: Web browsers and mobile applications for end-user interaction

### Data Flow

1. Hookah devices collect sensor data (temperature, humidity, smoke density, etc.)
2. Devices send telemetry data to Azure IoT Hub via MQTT or AMQP protocols
3. IoT Hub routes messages to the appropriate processing components
4. Processing services analyze, transform, and store the data in the database
5. The web application retrieves and displays the processed data
6. Users can send commands back to devices through the web interface

## Project Structure

The solution contains the following projects:

### Core Components

- **smartHookah**: 
  - Main ASP.NET web application providing the user interface and API endpoints
  - Implements MVC pattern with WebAPI controllers for RESTful services
  - Features OAuth 2.0 authentication with token-based security
  - Uses SignalR for real-time communication with clients
  - Includes Swagger for API documentation and testing
  - Contains responsive UI built with Bootstrap and Material Design

- **smartHookahCommon**: 
  - Shared library containing common functionality and models
  - Defines data transfer objects (DTOs) for cross-component communication
  - Implements common utilities, extensions, and helper classes
  - Contains shared business logic and validation rules
  - Provides error handling and exception management

### Device Communication

- **IotProcessDeviceToCloud**: 
  - IoT component for processing device-to-cloud messages
  - Runs on Windows IoT Core for edge processing
  - Implements device authentication and secure communication
  - Handles message serialization/deserialization
  - Provides local buffering for offline scenarios
  - Manages device connectivity and reconnection logic

- **ProcessDeviceToCloudMessages**: 
  - Azure WebJob for processing device messages in the cloud
  - Implements the Event Processor Host pattern for scalable message processing
  - Handles message routing based on device type and message content
  - Performs data transformation and enrichment
  - Implements retry logic and dead-letter handling
  - Stores processed data in the database via Entity Framework

### Development and Testing Tools

- **DeviceEmulator**: 
  - WPF application for simulating hookah device behavior
  - Provides a graphical interface for configuring virtual devices
  - Simulates various sensor readings and device states
  - Implements the same communication protocols as real devices
  - Supports scenario-based testing with predefined data patterns
  - Allows manual triggering of events and error conditions

- **KeyEmulator**: 
  - Utility for keyboard and mouse operation simulation
  - Supports automated UI testing scenarios
  - Implements low-level input device simulation
  - Provides programmatic control of input events
  - Used for integration testing of the user interface

- **MessagesProcessor**: 
  - Service for processing and routing messages within the system
  - Implements message queue patterns for reliable delivery
  - Provides message transformation and protocol adaptation
  - Supports multiple message formats and protocols
  - Handles message prioritization and throttling
  - Implements logging and monitoring of message flow

- **smartHookahTests**: 
  - Comprehensive test suite for the system components
  - Contains unit tests for individual components
  - Implements integration tests for component interactions
  - Provides mocks and stubs for external dependencies
  - Supports continuous integration with automated test runs

## Technologies Used

### Backend
- **ASP.NET MVC/WebAPI**: Framework for building the web application and RESTful services
- **C#**: Primary programming language for all server-side components
- **Entity Framework**: ORM for database access and management
- **Autofac**: Dependency injection container for modular architecture
- **Hangfire**: Background job processing for scheduled tasks

### Frontend
- **JavaScript/TypeScript**: Client-side scripting and application logic
- **Bootstrap**: Responsive UI framework for cross-device compatibility
- **Material Design**: UI component library for modern look and feel
- **Webpack**: Module bundler for JavaScript and asset optimization
- **Babel**: JavaScript compiler for cross-browser compatibility

### Cloud Services
- **Azure IoT Hub**: Managed service for bi-directional device communication
- **Azure WebJobs**: Background processing infrastructure
- **Azure Service Bus**: Message queuing for reliable communication
- **Azure Blob Storage**: Storage for device firmware and large data sets
- **Azure Application Insights**: Monitoring and diagnostics

### Real-time Communication
- **SignalR**: Library for real-time web functionality
- **MQTT**: Lightweight messaging protocol for IoT devices
- **AMQP**: Advanced message queuing protocol for reliable messaging

### Authentication & Security
- **OAuth 2.0**: Industry-standard protocol for authorization
- **JWT**: JSON Web Tokens for secure information transmission
- **Azure Active Directory**: Identity management (optional integration)
- **X.509 Certificates**: Device authentication and secure communication

### Documentation & Testing
- **Swagger/OpenAPI**: API documentation and testing
- **MSTest**: Testing framework for unit and integration tests
- **Moq**: Mocking framework for isolated testing
- **Selenium**: Web browser automation for UI testing

## Setup and Installation

### Prerequisites

- Visual Studio 2019 or later
- .NET Framework 4.7.2 or later
- SQL Server 2016 or later (local or remote)
- Azure subscription (for cloud features)
- Node.js 12+ and npm (for frontend development)
- Windows IoT Core (for edge device deployment)

### Configuration

1. Clone the repository
2. Open the solution in Visual Studio
3. Restore NuGet packages
4. Install npm packages:
   ```
   cd smartHookah
   npm install
   ```
5. Update connection strings in Web.config and App.config files:
   - `DefaultConnection`: SQL Server database connection
   - `AzureIotHub`: IoT Hub connection string
   - `AzureStorage`: Storage account connection string
   - `ServiceBus`: Service Bus connection string

6. Configure Azure services:
   - Create an IoT Hub instance in Azure Portal
   - Set up consumer groups for different processors
   - Configure message routes and endpoints
   - Set up Azure Storage accounts for device data
   - Deploy WebJobs to Azure App Service

### Database Setup

1. Open Package Manager Console in Visual Studio
2. Set the Default project to `smartHookah`
3. Run Entity Framework migrations to create the database:
   ```
   Update-Database -Verbose
   ```
4. Verify database creation in SQL Server Management Studio
5. Seed initial data (optional):
   ```
   Update-Database -TargetMigration "SeedData"
   ```

### Running the Application

1. Configure multiple startup projects in Visual Studio:
   - Right-click the solution in Solution Explorer
   - Select "Set StartUp Projects..."
   - Choose "Multiple startup projects"
   - Set `smartHookah` and `DeviceEmulator` to "Start"

2. Build the solution (Ctrl+Shift+B)
3. Press F5 to run the application
4. Access the web interface at `http://localhost:port`
5. Use the DeviceEmulator to simulate device connections

### Deploying to Production

1. Publish the web application to Azure App Service
2. Deploy WebJobs to the same App Service or separate instances
3. Configure IoT Hub for production scale
4. Set up monitoring and alerts
5. Deploy edge components to production devices

## Usage

### Web Interface

The web interface provides access to:

#### Dashboard
- Real-time device status monitoring
- Key performance indicators and metrics
- Interactive charts and visualizations
- Alert notifications and system status

#### Device Management
- Device registration and provisioning
- Firmware updates and configuration
- Remote command execution
- Device grouping and tagging
- Geolocation tracking and mapping

#### User Management
- User registration and authentication
- Role-based access control
- Permission management
- User activity logging and auditing

#### Session Management
- Hookah session scheduling and tracking
- Session history and playback
- Usage statistics and patterns
- Custom session parameters and profiles

#### Analytics
- Historical data analysis
- Usage trends and patterns
- Performance optimization recommendations
- Custom report generation
- Data export in multiple formats

#### System Configuration
- Global system settings
- Integration with external systems
- Notification rules and channels
- Backup and recovery options

### Device Emulator

For development and testing:

1. Set the startup project to `DeviceEmulator`
2. Configure connection settings:
   - IoT Hub connection string
   - Device ID and authentication
   - Message frequency and patterns
   - Sensor simulation parameters

3. Available simulation features:
   - Temperature sensor simulation with configurable range
   - Humidity sensor with random or patterned fluctuations
   - Smoke density simulation with customizable profiles
   - Battery level monitoring and discharge patterns
   - Connection quality simulation (packet loss, latency)
   - Error condition simulation (sensor failure, connectivity issues)

4. Run the emulator to simulate device behavior:
   - Start/stop individual sensor simulations
   - View outgoing and incoming messages
   - Save and load simulation profiles
   - Export simulation data for analysis

### API Integration

The system provides a comprehensive RESTful API for integration with external systems:

1. Authentication:
   ```
   POST /api/token
   Content-Type: application/x-www-form-urlencoded
   
   grant_type=password&username=user&password=pass
   ```

2. Device data retrieval:
   ```
   GET /api/devices
   Authorization: Bearer {token}
   ```

3. Command execution:
   ```
   POST /api/devices/{deviceId}/commands
   Authorization: Bearer {token}
   Content-Type: application/json
   
   {
     "commandType": "setTemperature",
     "parameters": {
       "temperature": 180
     }
   }
   ```

4. Webhook integration for external notifications
5. Batch operations for multiple devices
6. Streaming endpoints for real-time data

## Development

### System Architecture Details

The Smart Hookah system implements several architectural patterns:

1. **Microservices**: Each component focuses on specific functionality
2. **Event-driven architecture**: Components communicate via messages
3. **Repository pattern**: For data access abstraction
4. **CQRS**: Command Query Responsibility Segregation for complex operations
5. **MVC/MVVM**: For UI implementation and separation of concerns

### Adding New Features

1. Create a feature branch from `main`:
   ```
   git checkout -b feature/your-feature-name
   ```

2. Implement and test your changes:
   - Follow the existing architecture patterns
   - Add appropriate unit and integration tests
   - Update documentation as needed
   - Ensure backward compatibility

3. Submit a pull request for review:
   - Provide a detailed description of changes
   - Reference any related issues
   - Ensure all tests pass
   - Address code review feedback

### Coding Standards

- Follow Microsoft's C# coding conventions
- Use dependency injection for loosely coupled components
- Implement proper exception handling and logging
- Follow the SOLID principles of object-oriented design
- Use async/await for asynchronous operations
- Write unit tests for all new functionality (aim for >80% coverage)
- Document public APIs with XML comments
- Use meaningful variable and method names
- Keep methods small and focused on a single responsibility
- Use proper layer separation (data, business logic, presentation)

### Continuous Integration

The project uses Azure DevOps for CI/CD:

1. Automated builds on commit
2. Unit and integration test execution
3. Code quality analysis with SonarQube
4. Automated deployment to staging environments
5. Release management for production deployments

## License

[Specify your license information here]

## Contact

[Your contact information]
