/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package movieplayer.output;

import com.fazecast.jSerialComm.SerialPort;
import java.io.IOException;
import java.io.ObjectOutputStream;
import java.io.PrintWriter;
import java.util.logging.Level;
import java.util.logging.Logger;
import movieplayer.hardware.ActuatorSignal;

/**
 *
 * @author Sasa2905
 */
public class SerialCommunication {

    static SerialPort chosenPort;
    PrintWriter writer;

    public SerialCommunication() {
        initCommunication();
    }

    private void initCommunication() {
        SerialPort[] portNames = SerialPort.getCommPorts();
        SerialPort chosenPort = null;
        System.out.println(portNames.length);
        for (int i = 0; i < portNames.length; i++) {
            if (portNames[i].getDescriptivePortName().contains("Flora")) {
                chosenPort = portNames[i];
                break;
            }
        }
        if (chosenPort != null) {
            chosenPort.setComPortTimeouts(SerialPort.TIMEOUT_SCANNER, 0, 0);
            if (chosenPort.openPort()) {
                System.out.println("Port opened");
                writer = new PrintWriter(chosenPort.getOutputStream());
            }
        }
    }

    public void sendSignal(ActuatorSignal signal) {
        //System.out.println(signal.toString());
        String sending = signal.toString();
        writer.print(sending);
        writer.flush();
    }
}
