# Use an official Maven runtime as a parent image
FROM maven:3.6.3-openjdk-17

# Set the working directory in the container
WORKDIR /usr/src/app

# Copy the project files into the container at /usr/src/app
COPY . .

# Build the project and package the jar (including dependencies)
RUN mvn package

# Run the jar file
CMD ["java", "-jar", "target/healthchecker-1.0-SNAPSHOT-jar-with-dependencies.jar"]
