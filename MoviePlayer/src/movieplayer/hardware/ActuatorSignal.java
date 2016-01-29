/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package movieplayer.hardware;

/**
 *
 * @author Sasa2905
 */
public class ActuatorSignal {

    private int id;
    //Start-time of signal in ms
    private double startTime;
    //End-time of signal in ms
    private double endTime;
    //Speed of vibration
    private int speed;
    //Intensity of vibration
    private int intensity;
    //If signal has started or not;
    private boolean started;
    //position of actuator on body;
    private int position;
    //type of actuator
    private Actuator actuatorType;

    public ActuatorSignal(int id, double startTime, double endTime, int speed, int intensity) {
        this.id = id;
        this.startTime = startTime;
        this.endTime = endTime;
        this.speed = speed;
        this.intensity = intensity;
    }

    public double getStartTime() {
        return startTime;
    }

    public double getEndTime() {
        return endTime;
    }

    public int getSpeed() {
        return speed;
    }

    public int getIntensity() {
        return intensity;
    }

    public int checkTime(double time) {
        if (!started) {
            if (time >= this.startTime && time <= this.endTime) {
                System.out.println("START");
                started = true;
                return 1;
            }
            return 0;
        } else {
            if (time >= this.endTime && time >= this.startTime) {
                System.out.println("STOP");
                started = false;
                return 2;
            }
        }
        return 0;
    }

    @Override
    public String toString() {
        double duration = endTime - startTime;
        int state = 0;
        if(!started)
            state = 1;
        else
            state = 0;
        return String.format("%d|%f|%d|%d|%d|", id, duration, speed, intensity,state);
    }

}
