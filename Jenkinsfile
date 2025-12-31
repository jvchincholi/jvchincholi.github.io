pipeline {
    agent any
    environment {
        // Architect Note: Use variables for environment parity
        AZURE_REGISTRY = "yourregistry.azurecr.io"
        APP_NAME = "mymodernapi"
    }
    stages {
        stage('Checkout') {
            steps { checkout scm }
        }
        stage('Build Image') {
            steps {
                // Uses the Dockerfile we created earlier
                sh "docker build -t ${AZURE_REGISTRY}/${APP_NAME}:${env.BUILD_ID} ."
            }
        }
        stage('Push to Azure') {
            steps {
                // Login to Azure and push the 'Sealed Box'
                withCredentials([usernamePassword(credentialsId: 'azure-acr-creds', ...)]) {
                    sh "docker push ${AZURE_REGISTRY}/${APP_NAME}:${env.BUILD_ID}"
                }
            }
        }
        stage('Deploy to Azure') {
            steps {
                // Tell Azure to pull the new version
                sh "az webapp config container set --name ${APP_NAME} --docker-custom-image-name ${AZURE_REGISTRY}/${APP_NAME}:${env.BUILD_ID}"
            }
        }
    }
}