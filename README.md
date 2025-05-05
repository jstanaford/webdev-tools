# WebTechProfiler

A modern web application built with ASP.NET Core 8.0 and TailwindCSS for profiling and analyzing web technologies. This project provides a robust development environment using Docker containers for consistent deployment across different platforms.

## 🚀 Features

- ASP.NET Core 8.0 Web Application
- Modern UI with TailwindCSS
- Docker containerization
- Swagger/OpenAPI documentation
- Development environment management scripts

## 🛠️ Tech Stack

- **Backend**: ASP.NET Core 8.0
- **Frontend**: TailwindCSS
- **Containerization**: Docker & Docker Compose
- **API Documentation**: Swagger/OpenAPI
- **Development Tools**: npm, .NET SDK

## 📋 Prerequisites

- Docker and Docker Compose
- .NET 8.0 SDK
- Node.js and npm
- Git

## 🚀 Getting Started

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/webdev-tools.git
   cd webdev-tools
   ```

2. **Start the development environment**
   ```bash
   ./manage.sh start
   ```
   The application will be available at http://localhost:8083

## 🎮 Development Commands

The project includes a management script (`manage.sh`) with the following commands:

- `./manage.sh start` - Start the development environment
- `./manage.sh stop` - Stop the development environment
- `./manage.sh restart` - Restart the development environment
- `./manage.sh clean` - Clean up Docker containers and build artifacts
- `./manage.sh logs` - Show container logs
- `./manage.sh status` - Show container status

## 🏗️ Project Structure

```
webdev-tools/
├── src/
│   ├── WebTechProfiler/        # Main application code
│   ├── package.json            # Node.js dependencies
│   ├── tailwind.config.js      # TailwindCSS configuration
│   └── WebTechProfiler.csproj  # .NET project file
├── Dockerfile                  # Docker configuration
├── docker-compose.yml          # Docker Compose configuration
├── manage.sh                   # Development environment management script
└── README.md                   # Project documentation
```

## 🔧 Development

### Building the Frontend

The project uses TailwindCSS for styling. To build the CSS:

```bash
cd src
npm run tailwind:build
```

### Running Tests

```bash
dotnet test
```

## 📝 License

This project is licensed under the terms included in the LICENSE file.

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📫 Contact

For any questions or concerns, please open an issue in the GitHub repository.
