#!/bin/bash

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Function to display usage
show_usage() {
    echo -e "${GREEN}Usage: ./manage.sh <command>${NC}"
    echo "Commands:"
    echo "  start     - Start the development environment"
    echo "  stop      - Stop the development environment"
    echo "  restart   - Restart the development environment"
    echo "  clean     - Clean up Docker containers and build artifacts"
    echo "  logs      - Show container logs"
    echo "  status    - Show container status"
}

# Function to check if Docker is running
check_docker() {
    if ! docker info > /dev/null 2>&1; then
        echo -e "${RED}Error: Docker is not running${NC}"
        exit 1
    fi
}

case "$1" in
    start)
        check_docker
        echo -e "${GREEN}Starting development environment...${NC}"
        docker-compose up --build -d
        echo -e "${GREEN}Development environment is running at http://localhost:8083${NC}"
        ;;
    stop)
        check_docker
        echo -e "${GREEN}Stopping development environment...${NC}"
        docker-compose down
        ;;
    restart)
        check_docker
        echo -e "${GREEN}Restarting development environment...${NC}"
        docker-compose down
        docker-compose up --build -d
        echo -e "${GREEN}Development environment is running at http://localhost:8083${NC}"
        ;;
    clean)
        check_docker
        echo -e "${GREEN}Cleaning up development environment...${NC}"
        docker-compose down --volumes --remove-orphans
        rm -rf src/bin src/obj
        echo -e "${GREEN}Cleanup complete${NC}"
        ;;
    logs)
        check_docker
        docker-compose logs -f
        ;;
    status)
        check_docker
        docker-compose ps
        ;;
    *)
        show_usage
        exit 1
        ;;
esac 