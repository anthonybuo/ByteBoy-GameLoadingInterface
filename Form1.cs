using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace GameLoadingInterface
{
    public partial class Form1 : Form
    {

        // MUST DEFINE ROM FILENAMES HERE

        const byte MCU_signal_byte = 0x55;

        public enum Game
        {
            PONG,
            SPACE_INVADERS,
            TETRIS,
            BREAKOUT,
            TEST_OPCODE,
            CONNECT4,
            INVALID,
        };

        public Dictionary<Game, string> games = new Dictionary<Game, string>
        {
            { Game.PONG, pong_filename },
            { Game.SPACE_INVADERS, space_invaders_filename },
            { Game.TETRIS, tetris_filename },
            { Game.BREAKOUT, breakout_filename },
            { Game.TEST_OPCODE, test_opcode_filename },
            { Game.CONNECT4, connect4_filename }
        };

        Game current_game = Game.INVALID;

        public Form1()
        {
            InitializeComponent();
            buttonUpload.Enabled = false;
        }

        int game_data_length;
        byte[] game_data;

        private void ReadProgram()
        {
            var fs = new FileStream(games[current_game], FileMode.Open);
            game_data_length = (int)fs.Length;
            Console.WriteLine("Opened a game that is " + game_data_length.ToString() + " bytes large.");
            game_data = new byte[game_data_length];
            fs.Read(game_data, 0, game_data_length);

            /* Memory dump */
            for (int ix = 0; ix < game_data_length; ix += 16)
            {
                var cnt = Math.Min(16, game_data_length - ix);
                var line = new byte[cnt];
                Array.Copy(game_data, ix, line, 0, cnt);
                // Write address + hex + ascii
                Console.Write("{0:X6}  ", ix);
                Console.Write(BitConverter.ToString(line));
                Console.Write("  ");
                // Convert non-ascii characters to .
                for (int jx = 0; jx < cnt; ++jx)
                    if (line[jx] < 0x20 || line[jx] > 0x7f) line[jx] = (byte)'.';
                Console.WriteLine(Encoding.ASCII.GetString(line));
            }
            Console.ReadLine();
        }

        private void SendGameFile()
        {
            if (serialPort1.IsOpen)
            {
                byte[] header = { 255, (byte)(game_data_length >> 8), (byte)(game_data_length & 0x00FF) };
                serialPort1.Write(header, /*offset=*/0, /*num_bytes=*/3);
                serialPort1.Write(game_data, /*offset=*/0, /*num_bytes=*/game_data_length);
            }
        }

        private void buttonUpload_MouseClick(object sender, MouseEventArgs e)
        {
            if (current_game == Game.INVALID || !serialPort1.IsOpen)
            {
                return;
            }
            ReadProgram();
        }

        private void checkBoxPong_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxPong.Checked)
            {
                current_game = Game.PONG;
                buttonUpload.Enabled = true;
                checkBoxSpaceInvaders.Checked = false;
                checkBoxTetris.Checked = false;
                checkBoxBreakout.Checked = false;
                checkBoxTestOpcode.Checked = false;
                checkBoxConnect4.Checked = false;
            }

        }

        private void checkBoxSpaceInvaders_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxSpaceInvaders.Checked)
            {
                current_game = Game.SPACE_INVADERS;
                buttonUpload.Enabled = true;
                checkBoxPong.Checked = false;
                checkBoxTetris.Checked = false;
                checkBoxBreakout.Checked = false;
                checkBoxTestOpcode.Checked = false;
                checkBoxConnect4.Checked = false;
            }

        }

        private void checkBoxTetris_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxTetris.Checked)
            {
                current_game = Game.TETRIS;
                buttonUpload.Enabled = true;
                checkBoxSpaceInvaders.Checked = false;
                checkBoxPong.Checked = false;
                checkBoxBreakout.Checked = false;
                checkBoxTestOpcode.Checked = false;
                checkBoxConnect4.Checked = false;
            }

        }

        private void checkBoxBreakout_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxBreakout.Checked)
            {
                current_game = Game.BREAKOUT;
                buttonUpload.Enabled = true;
                checkBoxSpaceInvaders.Checked = false;
                checkBoxTetris.Checked = false;
                checkBoxPong.Checked = false;
                checkBoxTestOpcode.Checked = false;
                checkBoxConnect4.Checked = false;
            }

        }

        private void checkBoxTestOpcode_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxTestOpcode.Checked)
            {
                current_game = Game.TEST_OPCODE;
                buttonUpload.Enabled = true;
                checkBoxSpaceInvaders.Checked = false;
                checkBoxTetris.Checked = false;
                checkBoxPong.Checked = false;
                checkBoxBreakout.Checked = false;
                checkBoxConnect4.Checked = false;
            }

        }
        private void comboBoxCOMPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            serialPort1.PortName = comboBoxCOMPort.SelectedItem.ToString();
        }

        private void buttonCOMPort_MouseClick(object sender, MouseEventArgs e)
        {
            // open/close serial port
            if (!serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Open();
                    buttonCOMPort.Text = "Close Serial";
                } catch
                {
                    // unhandled error
                }
            } else
            {
                serialPort1.Close();
                buttonCOMPort.Text = "Connect Serial";
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Update COM box
            comboBoxCOMPort.Items.Clear();
            comboBoxCOMPort.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
            if (comboBoxCOMPort.Items.Count == 0)
            {
                comboBoxCOMPort.Text = "No COM ports!";
            } else
            {
                comboBoxCOMPort.SelectedIndex = 0;
            }
        }

        private void serialPort1_DataReceived_1(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            int new_byte = 0;
            int bytes_to_read = serialPort1.BytesToRead;
            while (bytes_to_read > 0)
            {
                new_byte = serialPort1.ReadByte();
                // Check if MCU is signaling a new game download
                if (new_byte == MCU_signal_byte)
                {
                    SendGameFile();
                }
                bytes_to_read = serialPort1.BytesToRead;
            }

        }

        private void checkBoxConnect4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxConnect4.Checked)
            {
                current_game = Game.CONNECT4;
                buttonUpload.Enabled = true;
                checkBoxSpaceInvaders.Checked = false;
                checkBoxTetris.Checked = false;
                checkBoxPong.Checked = false;
                checkBoxBreakout.Checked = false;
            }

        }
    }
}
