pipeline {
  agent any

  options {
    timestamps()
    disableConcurrentBuilds()
  }

  environment {
    APP_NAME        = "greendragontrading-be"
    DEPLOY_BRANCH   = "deploy"
    DOCKERFILE_PATH = "GreenDragonTrading.Api/Dockerfile"
    CONTAINER_NAME  = "greendragontrading-be-deploy"
    DOCKER_NETWORK  = "at-net"
    ENV_CRED_ID     = "env-chung-khoan-be"
  }

  stages {

    stage('Guard: deploy branch only') {
      steps {
        script {
          echo "[Init] Detected branch: ${env.BRANCH_NAME}"
          if (env.BRANCH_NAME != env.DEPLOY_BRANCH) {
            currentBuild.result = 'NOT_BUILT'
            error("This pipeline only deploys branch '${env.DEPLOY_BRANCH}'. Current branch: ${env.BRANCH_NAME}")
          }
          env.IMAGE_TAG = "${env.APP_NAME}:deploy-${env.BUILD_NUMBER}"
          echo "[Init] IMAGE_TAG = ${env.IMAGE_TAG}"
        }
      }
    }

    stage('Checkout') {
      steps {
        checkout scm
        echo "[Checkout] Code checked out."
      }
    }

    stage('Build Docker Image') {
      steps {
        echo "[Build] Building Docker image: ${env.IMAGE_TAG}"
        sh """
          set -e
          docker build -f "${DOCKERFILE_PATH}" -t "${IMAGE_TAG}" .
        """
        echo "[Build] Completed → ${env.IMAGE_TAG}"
      }
    }

    stage('Test & Scan (optional)') {
      steps {
        echo "[Test] Add dotnet test / security scan here if needed."
      }
    }

    stage('Deploy') {
      steps {
        withCredentials([file(credentialsId: env.ENV_CRED_ID, variable: 'ENV_FILE')]) {
          sh """
            set -e

            echo "--- Preparing env file for docker run"
            cp "\$ENV_FILE" ./.env.deploy

            echo "--- Ensure network exists: ${DOCKER_NETWORK}"
            docker network inspect "${DOCKER_NETWORK}" >/dev/null 2>&1 || docker network create "${DOCKER_NETWORK}"

            echo "--- Stop/remove old container (if any): ${CONTAINER_NAME}"
            docker rm -f "${CONTAINER_NAME}" 2>/dev/null || true

            echo "--- Run new container: ${CONTAINER_NAME}"
            docker run -d \
              --name "${CONTAINER_NAME}" \
              --restart unless-stopped \
              --env-file ./.env.deploy \
              --network "${DOCKER_NETWORK}" \
              "${IMAGE_TAG}"

            rm -f ./.env.deploy
            echo "--- Deploy OK"
          """
        }
      }
    }

    stage('Cleanup') {
      steps {
        sh """
          set +e
          echo "--- Cleaning old images for repo ${APP_NAME} (keep current: ${IMAGE_TAG})"
          OLD_IMAGES=\$(docker images --format "{{.Repository}}:{{.Tag}}" | grep "^${APP_NAME}:" | grep -v "${IMAGE_TAG}" || true)

          for IMG in \$OLD_IMAGES; do
            echo "Deleting: \$IMG"
            docker rmi -f "\$IMG" || true
          done

          docker image prune -f || true
        """
      }
    }
  }

  post {
    always {
      echo "Pipeline finished for branch ${env.BRANCH_NAME}."
    }
  }
}
