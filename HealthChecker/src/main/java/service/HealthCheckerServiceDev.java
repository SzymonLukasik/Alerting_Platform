package service;

public class HealthCheckerServiceDev {
    public static void main(String[] args) {
        // set sysprop to dev
        System.setProperty("ENVIRONMENT", "DEV_ALEKS_PROJECT");
        HealthCheckerService.start();
    }
}