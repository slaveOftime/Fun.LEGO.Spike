/*
 * Serial.js
 * By: Gabriel Sessions
 * Last Edit: 8/2/2022
 * 
 * Component that handles the Serial connection between the website and the 
 * python microprocessor.
 * 
 */ 

import React, {useState, useEffect} from "react";
import SerialButton from "./SerialButton";

import Tooltip from "@mui/material/Tooltip";
import PlayCircleIcon from '@mui/icons-material/PlayCircle';
import StopCircleIcon from '@mui/icons-material/StopCircle';
import RestartAltIcon from '@mui/icons-material/RestartAlt';
import Fab from '@mui/material/Fab';

// Serial Variables

const VENDOR_ID = 0x0694; // LEGO SPIKE Prime Hub

let port = null;

export const CONTROL_A = '\x01'; // CTRL-A character
export const CONTROL_B = '\x02'; // CTRL-A character
export const CONTROL_C = '\x03'; // CTRL-C character 
export const CONTROL_D = '\x04'; // CTRL-D character
export const CONTROL_E = '\x05'; // CTRL-E character
export const ENTER = '\r\n' // NEWLINE character
export const TAB = '\x09' // TAB character

const docsLink = "https://spike3-docs.web.app/";

let isWriteInit = false;
let textEncoder;
let writableStreamClosed;

let reader;
let writer;
let lockedReader = false;

function Serial(props) {

    /**
     * Attempts to open a serial connection between the site and a serial device
     * @returns True if initialzation completes without error, false otherwise
     */
    async function initWebSerial() {
        await navigator.serial.getPorts();

        port = await navigator.serial.requestPort();

        // wait for the port to open.
        try {
            await port.open({ baudRate: 115200 });
        }
        catch (er) {
            console.log("%cTuftsCEEO ", "color: #3ba336;", er);
            //await port.close();
        }

        if (port.readable) {
            props.setWritePort(() => {
                return ((value) => writeToPort(value));
            });
            return true;
        }
        else {
            return false;
        }
    }

    /**
     * Reads data from an initialized serial port and pipes the output
     * to the IDE console using pipeToOutput().
     */
    async function readPort() {
        // eslint-disable-next-line no-undef
        let decoder = new TextDecoderStream();

        let inputDone = port.readable.pipeTo(decoder.writable);
        const inputStream = decoder.readable;

        reader = inputStream.getReader();
        lockedReader = true;
        while (port.readable && lockedReader) {
            //const reader = port.readable.getReader();
            try {
                while (true) {
                    const { value, done } = await reader.read();
                    if (done) {
                        console.log("done!")
                        break;
                    }
                    props.exportConsole(value);
                    console.log(value)
                }
            } catch (error) {
                console.error(error);
            } finally {
                reader.releaseLock();
            }
        }
    }

    /**
     * Initializes a write stream so user input can be written to the 
     * serial port connection
     */
    function initWriteStream() {
        // eslint-disable-next-line no-undef
        textEncoder = new TextEncoderStream();
        writableStreamClosed = textEncoder.readable.pipeTo(port.writable);
        writer = textEncoder.writable.getWriter();
    }

    

    /**
     * Writes an array of strings (lines) to the REPL
     * @param {Array<string>} lines - An array of REPL commands to be written
     * to the device REPL
     */
    async function writeToPort(lines) {
        if (!isWriteInit) {
            try {
                initWriteStream()
                isWriteInit = true;
            }
            catch (e) {
                console.error("Unable to initialize stream")
            }
        }

        
        try {
            await writer.write(CONTROL_C);
            console.log("----------------WRITING TO MICROPROCESSOR----------------")
            console.log(lines)

            if (typeof(lines) === "string")
                lines = lines.split("\n");
            // Writes code one line at a time
            if (typeof(lines) === "object") {
                await lines.forEach(async (element) => {
                    await writer.write(element + "\r\n");
                });
                
            }

            console.log("----------------FINISHED WRITING----------------")

        }
        catch(e) {
            console.log("Writing Error!")
            console.error(e);
        }
        
    }

    /**
     * Writes a CRTL+C to exit any currently running program and starts
     * reading from the serial port
     */
    async function startWebSerial() {
        if (await initWebSerial()) {
            await writeToPort([CONTROL_C]);
            setTimeout(async() => {
                readPort()
            }, 250);
        }
    }

    /**
     * Release locks on serial text streams so the port can be disconnected
     * BUG: works inconsistently
     */
    async function unlockStreams() {
        /*
        reader.cancel();
        reader.closed.then(() => {
            lockedReader = false;
        });
       isWriteInit = false;
       */
        
    }

    // Unlocks streams and then closes the serial port
    async function closePort() {
        /*
        unlockStreams();
        let interval = setInterval(() => {
            if (!(lockedReader || isWriteInit)) {
                port.close();
                setSerialOn(false);
                setConnectText(defaultDirections);
                
                isWriteInit = false;
                textEncoder = undefined;
                writableStreamClosed = undefined;

                reader = undefined;
                writer = undefined;
                lockedReader = false;
                clearInterval(interval);
                
            }
        }, 100);
        */

    }
    
    // Serial Port UI Component Hooks
    const defaultDirections = "";
    const activeSerialDirections = ""
    const [connectText, setConnectText] = useState(defaultDirections);
    const [serialOn, setSerialOn] = useState(false);
   

    /**
     * Shows all avaliable serial connection options (run code/stop code/etc.)
     * when a serial connection is established, 
     * hides them when the port is disconnected.
     */
    function serialButtonConnected() {
        setSerialOn(true);
        setConnectText(activeSerialDirections);
        port.addEventListener('disconnect', event => {
            setSerialOn(false);
            setConnectText(defaultDirections)
        });
        
    }

    /**
     * Initializes an attempt to establish a WebSerial connection
     * (occurs when the serial button is pressed in disconnected state)
     */
    async function connectToSPIKE() {
        await startWebSerial(); 
        serialButtonConnected();
    }

    /**
     * Writes the text stored in the currently active IDE editor
     * to the serial port
     * @returns Nothing
     */
    function runCurrentCode() {
        let currentCode = props.getCurrentCode()

        if (!currentCode.includes("\n")) {
            writeToPort(currentCode + "\r\n");
            return;
        }

        // Implement raw REPL with
        // CTRL-A -> Code -> CTRL-D -> CTRL-B

        writeToPort(CONTROL_E)
        writeToPort(currentCode);
        writeToPort(CONTROL_D)
        //writeToPort(CONTROL_B)
    }

    // Old Function, may be useful later
    // Removes all comments from code before parsing
    // Includes #, ''', and empty line comments
    function stripComments(codeArray) {
        let blockCommentMode = false;
        return codeArray.filter((line) => {
            if (line.length === 0 || line.trim().charAt(0) === "#") {
                return false;
            }
            if (line.length >= 3 && line.includes("'''")) {
                if (blockCommentMode) {
                    blockCommentMode = false;
                } 
                else {
                    blockCommentMode = true;
                }
                return false;
            }
            if (blockCommentMode) {
                return false;
            }
            return true;
        })

    }

    /**
     * Saves code in current editor to the SPIKE Prime as a file
     */
    function uploadCurrentCode() {
        const code = props.getCurrentCode();
        const fileName = props.getCurrentFileName();
        writeToPort([CONTROL_C, CONTROL_E, 'code = """' + code + '"""\n', 'f = open("' + fileName + '", "w")\n', 'f.write(code)\n', 'f.close()\n', CONTROL_D])

    }

    /**
     * Runs the file with the name matching the current IDE editor tab.
     */
    function runCurrentFile() {
        const fileName = props.getCurrentFileName();
        writeToPort(["f = open('" + fileName + "')\r\n", "exec(f.read())\r\n", "f.close()\r\n"])
    }

    /**
     * Uploads the current editor code to the device over serial and then 
     * executes the uploaded file.
     */
    function writeAndRunCode() {
        uploadCurrentCode();
        runCurrentFile();
    }

    /**
     * Downloads the code in the current active tab as a ".py"
     * file saved to the user's local file system.
     */
    function downloadTxtFile() {
        const element = document.createElement("a");
        const file = new Blob([props.getCurrentCode()], {
          type: "text/plain"
        });
        element.href = URL.createObjectURL(file);
        element.download = props.getCurrentFileName();
        document.body.appendChild(element);
        element.click();
    }


    
    
    

    return (
        <div className="flex justify-center">
            {/* BUG: Condense Buttons into a single component */}
            <SerialButton 
                on={serialOn} 
                offColor="error"
                onColor="success" 
                connectText={connectText} 
                
                onClick={() => {
                    connectToSPIKE();
                }} 
                restartClick={closePort} 
                getCurrentCode={props.getCurrentCode}
                runCurrentCode={writeToPort}
            />
            
            {/*
            <div className={!serialOn ? "hidden" : "mx-4"}>
                <Tooltip title="Restart Device" placement="top">
                    <Fab 
                        onClick={async () => {
                            await writeToPort(CONTROL_D);
                            setTimeout(() => {
                                writeToPort(CONTROL_C);
                            }, 1000);
                        }} 
                        color="warning" 
                        aria-label="add" 
                        size="small">

                        <RestartAltIcon />
                    </Fab>
                </Tooltip>
            </div>
            */}


            <div className={!serialOn ? "hidden" : "mx-4"}>
                <Tooltip title="Stop" placement="top">
                    <Fab 
                        onClick={() => {
                            writeToPort([CONTROL_C])
                        }} 
                        color="error" 
                        aria-label="add" 
                        size="small">

                        <StopCircleIcon />
                    </Fab>
                </Tooltip>
            </div>


            
        </div>
        
    )
}

export default Serial;