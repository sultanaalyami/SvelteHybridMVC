#!/bin/bash

# ========================================
# HRCE Deployment Script
# Deploys application to Docker/Kubernetes
# ========================================

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Configuration
DEPLOY_TARGET="${DEPLOY_TARGET:-docker}"  # docker, kubernetes, local
ENVIRONMENT="${ENVIRONMENT:-production}"
IMAGE_TAG="${IMAGE_TAG:-latest}"
REGISTRY="${REGISTRY:-}"

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}HRCE Deployment Script${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""
echo -e "${YELLOW}Target:${NC} $DEPLOY_TARGET"
echo -e "${YELLOW}Environment:${NC} $ENVIRONMENT"
echo -e "${YELLOW}Image Tag:${NC} $IMAGE_TAG"
echo ""

# Function to deploy to Docker
deploy_docker() {
    echo -e "${BLUE}Deploying to Docker...${NC}"
    
    # Build image
    echo -e "${YELLOW}Building Docker image...${NC}"
    docker build -t hrce:$IMAGE_TAG .
    
    if [ $? -ne 0 ]; then
        echo -e "${RED}✗ Docker build failed${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}✓ Docker image built${NC}"
    
    # Tag for registry if specified
    if [ ! -z "$REGISTRY" ]; then
        echo -e "${YELLOW}Tagging image for registry...${NC}"
        docker tag hrce:$IMAGE_TAG $REGISTRY/hrce:$IMAGE_TAG
        
        echo -e "${YELLOW}Pushing to registry...${NC}"
        docker push $REGISTRY/hrce:$IMAGE_TAG
        
        echo -e "${GREEN}✓ Image pushed to registry${NC}"
    fi
    
    # Stop existing container
    if docker ps -a | grep -q hrce-prod; then
        echo -e "${YELLOW}Stopping existing container...${NC}"
        docker stop hrce-prod || true
        docker rm hrce-prod || true
    fi
    
    # Run new container
    echo -e "${YELLOW}Starting new container...${NC}"
    docker run -d \
        --name hrce-prod \
        --privileged \
        --cap-add=SYS_ADMIN \
        --cap-add=NET_ADMIN \
        -p 8080:8080 \
        -p 8081:8081 \
        -v hrce-data:/app/data \
        -e ASPNETCORE_ENVIRONMENT=$ENVIRONMENT \
        hrce:$IMAGE_TAG
    
    echo -e "${GREEN}✓ Container started${NC}"
    
    # Show logs
    echo ""
    echo -e "${YELLOW}Container logs:${NC}"
    docker logs -f hrce-prod
}

# Function to deploy to Kubernetes
deploy_kubernetes() {
    echo -e "${BLUE}Deploying to Kubernetes...${NC}"
    
    # Check if kubectl is installed
    if ! command -v kubectl &> /dev/null; then
        echo -e "${RED}Error: kubectl is not installed${NC}"
        exit 1
    fi
    
    # Build and push image
    if [ ! -z "$REGISTRY" ]; then
        echo -e "${YELLOW}Building and pushing image...${NC}"
        docker build -t $REGISTRY/hrce:$IMAGE_TAG .
        docker push $REGISTRY/hrce:$IMAGE_TAG
        echo -e "${GREEN}✓ Image built and pushed${NC}"
    else
        echo -e "${RED}Error: REGISTRY must be set for Kubernetes deployment${NC}"
        exit 1
    fi
    
    # Create namespace if it doesn't exist
    kubectl create namespace hrce --dry-run=client -o yaml | kubectl apply -f -
    
    # Apply Kubernetes manifests
    echo -e "${YELLOW}Applying Kubernetes manifests...${NC}"
    
    # Replace image tag in manifests
    sed "s|IMAGE_TAG|$IMAGE_TAG|g" k8s/deployment.yaml | \
    sed "s|REGISTRY|$REGISTRY|g" | \
    kubectl apply -f -
    
    kubectl apply -f k8s/service.yaml
    kubectl apply -f k8s/ingress.yaml
    
    echo -e "${GREEN}✓ Manifests applied${NC}"
    
    # Wait for rollout
    echo -e "${YELLOW}Waiting for rollout to complete...${NC}"
    kubectl rollout status deployment/hrce -n hrce
    
    echo -e "${GREEN}✓ Deployment completed${NC}"
    
    # Show status
    echo ""
    kubectl get pods -n hrce
}

# Function to deploy locally
deploy_local() {
    echo -e "${BLUE}Deploying locally...${NC}"
    
    # Build
    ./build.sh
    
    # Run
    echo -e "${YELLOW}Starting application...${NC}"
    
    export ASPNETCORE_ENVIRONMENT=$ENVIRONMENT
    dotnet run --no-build --configuration Release
}

# Main deployment logic
case $DEPLOY_TARGET in
    docker)
        deploy_docker
        ;;
    kubernetes|k8s)
        deploy_kubernetes
        ;;
    local)
        deploy_local
        ;;
    *)
        echo -e "${RED}Error: Unknown deployment target '$DEPLOY_TARGET'${NC}"
        echo -e "${YELLOW}Valid targets: docker, kubernetes, local${NC}"
        exit 1
        ;;
esac

echo ""
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}✓ Deployment completed successfully!${NC}"
echo -e "${GREEN}========================================${NC}"
