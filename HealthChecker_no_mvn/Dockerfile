# Use an official Java runtime as a parent image
FROM openjdk:11

# Set the working directory in the container
WORKDIR /usr/src/app

# Copy the current directory contents into the container at /usr/src/app
COPY . .

RUN ls
# Compile the Java program
RUN javac -cp ".;google-cloud-pubsub-1.113.5.jar;mysql-connector-java-8.0.23.jar" HealthChecker.java

# Run the Java program when the container launches
CMD ["java", "-cp", ".:google-cloud-pubsub-1.113.5.jar:mysql-connector-java-8.0.23.jar", "HealthChecker"]   