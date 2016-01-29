/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package movieplayer.output;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.io.IOException;
import java.io.PrintWriter;
import java.util.ArrayList;
import java.util.List;
import java.util.regex.Matcher;
import java.util.regex.Pattern;
import movieplayer.hardware.ActuatorSignal;

/**
 *
 * @author Sasa2905
 */
public class ActuatorFile {

    private List<ActuatorSignal> signals;
    private SerialCommunication communication;

    public ActuatorFile() {
        signals = new ArrayList<>();
        communication = new SerialCommunication();
    }

    public void createFile() throws FileNotFoundException, IOException {
        File signalFile = new File("fileOutput.txt");
        PrintWriter writer = new PrintWriter(signalFile);
        writer.write("HALLO");
    }

    public void decodeFile() throws FileNotFoundException, IOException {
        BufferedReader br = new BufferedReader(new FileReader("data.txt"));
        try {
            String line = br.readLine();
            while (line != null) {
                signals.add(createSignal(line));
                line = br.readLine();
            }
        } finally {
            //System.out.println("SIZE LIST " + signals.size());
            br.close();
        }
    }

    public ActuatorSignal createSignal(String info) {
        Pattern p = Pattern.compile("\\[(.+)\\]\\[(.+)\\]\\[(.+)\\]\\[(.)\\]\\[(.)\\]");
        //System.out.println(info);
        Matcher m = p.matcher(info);
        m.matches();
        int id = Integer.valueOf(m.group(1));
        double startTime = Double.valueOf(m.group(2).replace(":", "."));
        double endTime = Double.valueOf(m.group(3).replace(":", "."));
        int speed = Integer.valueOf(m.group(4));
        int intensity = Integer.valueOf(m.group(5));
        ActuatorSignal signal = new ActuatorSignal(id, startTime, endTime, speed, intensity);
        return signal;
    }

    public boolean checkTime(double time) {
        for (ActuatorSignal signal : signals) {
            if (signal.checkTime(time) == 1 || signal.checkTime(time) == 2)
                communication.sendSignal(signal);
        }
        return false;
    }
}
