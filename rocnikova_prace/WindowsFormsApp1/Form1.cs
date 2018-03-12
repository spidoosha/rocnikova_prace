/*
 * ROČNÍKOVÁ PRÁCE
 * DIALOGFLOW
 * I S PO
 * HOANG ANH TUAN, 8.M
 * PROGRAMOVACÍ SEMNINÁŘ II 2017/2018
*/
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ApiAiSDK;
using ApiAiSDK.Model;
using System.Speech.Recognition;
using System.Speech.Synthesis;

/*
 * 
 * Nainstaloval jsem si knihovnu api.ai .NET pomocí Nugetu - Package Manager Console příkazem "PM> Install-Package ApiAiSDK"
 * Nadefinoval jsem si ji pomocí: 
 * - using ApiAiSDK
 * - using ApiAiSDK.Model
 * Dále jsem nadefinoval knihovnu, pomocí které může moje aplikace "mluvit" a já můžu mluvit do ní:
 * - using System.Speech.Recognition
 * - using System.Speech.Synthesis
 * 
*/

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private static ApiAi apiAi;
        SpeechRecognitionEngine recEngine = new SpeechRecognitionEngine();
        SpeechSynthesizer synthesizer = new SpeechSynthesizer();

        // Vytvořil jsem si třídu Sandwich. Každý Sandwich má omáčku, 1-3 ingredience a počáteční nulovou cenu
        // K tomu list, do které si budu zapisovat všechny sendviče a int celkové ceny
        class Sandwich
        {
            public string sauce;
            public string ing;
            public string ing1;
            public string ing2;
            public int cena = 0;
        }
        List<Sandwich> list = new List<Sandwich>();
        int celkovaCena = 0;
        string odpoved;
        //Do této funkce vstupuje to, co nám uživatel řekne a rozhoduje, co aplikace dále odpoví
        void PrijmiOdpovedAOdpovez(string input)
        {
            // vkládáme do AIConfiguration token našeho agenta a oznamujeme mu, že konverzace probíhá v angličtině
            var config = new AIConfiguration("bfd6b02ea0ac450e9bb859c2ed1a1760", SupportedLanguage.English);
            apiAi = new ApiAi(config);

            // zde vkládáme vstup uživatele
            var vstup = apiAi.TextRequest(input);
            richTextBox1.Text += "You: " + input + '\n';
            // errorx;
            // PripravSandwich(vstup.Result.Parameters["Ingredients[0]"].ToString(), vstup.Result.Parameters["sauce"].ToString(), new Sandwich()); 
            Sandwich sendvic = new Sandwich();
            if (vstup.Result.Metadata.IntentName == "order_sandwich" || vstup.Result.Metadata.IntentName == "order_new_sandwich")
            {
                // Ingredience se musejí z vstup.Result.Parameters["Ingredients"].ToString() vyextrahovat
                PripravSandwich(vstup.Result.Parameters["Ingredients"].ToString(), vstup.Result.Parameters["sauce"].ToString(), sendvic);
                celkovaCena = celkovaCena + sendvic.cena;
                list.Add(sendvic);

                // Agent zrekapituluje poslední zadaný sendvič
                if (vstup.Result.ActionIncomplete == false)
                {
                    odpoved = "You have ordered " + sendvic.sauce + " sandwich with " + sendvic.ing + " " + sendvic.ing1 + " " + sendvic.ing2 + ". It will cost " + sendvic.cena + " Dialogflow coins. Would you like to get another one?";
                    synthesizer.SpeakAsync(odpoved);
                    richTextBox1.Text += "Agent: " + odpoved + '\n';
                }
                else
                {
                    synthesizer.SpeakAsync(vstup.Result.Fulfillment.Speech);
                    richTextBox1.Text += "Agent: " + vstup.Result.Fulfillment.Speech + '\n';
                }
            }
            // Agent zrekapituluje celou objednavku
            else if (vstup.Result.Metadata.IntentName == "say_goodbye")
            {
                
                odpoved = "You have ordered " + VypisSandwiche(list) + "It's " + celkovaCena + " Dialogflow coins.";
                synthesizer.SpeakAsync(odpoved);
                richTextBox1.Text += "Agent: " + odpoved + '\n';
                celkovaCena = 0;
            }
            else
            {
                // Odpoved je výchozí podle agenta
                synthesizer.SpeakAsync(vstup.Result.Fulfillment.Speech);
                richTextBox1.Text += "Agent: " + vstup.Result.Fulfillment.Speech + '\n';
            }
        }

        // Vypíše všechny sendviče
        static string VypisSandwiche(List<Sandwich> kredenc)
        {
            int i = 0;
            string vypis = "";
            foreach (Sandwich s in kredenc)
            {
                if (i < kredenc.Count)
                {
                    vypis += s.sauce + " sandwich with " + s.ing + " " + s.ing1 + " " + s.ing2 + ", ";
                }
                else
                {
                    vypis += "and " + s.sauce + " sandwich with " + s.ing + " " + s.ing1 + " " + s.ing2;
                }
                i++;
            }

            return vypis;
        }

        //Do Sandwich sandwich přidá hodnoty, nejdřív si musí ale vyextrahovat ingredience
        void PripravSandwich(string ingredience, string sauce, Sandwich sandwich)
        {
            if (sauce != "")
                sandwich.sauce = sauce;
            sandwich.cena = sandwich.cena + VypocitejUtratuOmacky(sandwich.sauce);
            bool zacni = false;

            bool konec = false;
            int NtaIng = 0;
            string ing = "";
            if (ingredience != "")
                foreach (char c in ingredience)
                {
                    if (zacni == true)
                    {
                        if (c == '"')
                        {
                            if (NtaIng == 0)
                            {
                                sandwich.ing = ing;
                                sandwich.cena = sandwich.cena + VypocitejUtratuIng(sandwich.ing);
                                NtaIng++;
                            }
                            else if (NtaIng == 1)
                            {
                                sandwich.ing1 = ing;
                                sandwich.cena = sandwich.cena + VypocitejUtratuIng(sandwich.ing1);
                                NtaIng++;
                            }
                            else if (NtaIng == 2)
                            {
                                sandwich.ing2 = ing;
                                sandwich.cena = sandwich.cena + VypocitejUtratuIng(sandwich.ing2);
                            }

                            ing = "";
                            zacni = false;
                            konec = true;
                        }
                        else
                        {
                            ing = ing + c;
                        }
                    }

                    if (c == '"')
                    {
                        if (konec == true)
                        {
                            konec = false;
                        }
                        else
                        {
                            zacni = true;
                        }
                    }
                }
            NtaIng = 0;

        }
        
        //Spočítá ceny omacky a ingredienci
        static int VypocitejUtratuOmacky(string sauce)
        {
            int utrata = 0;
            switch (sauce)
            {
                case "mayo":
                    utrata = 2;
                    break;
                case "ketchup":
                    utrata = 2;
                    break;
                case "chilli":
                    utrata = 3;
                    break;
                case "mustard":
                    utrata = 1;
                    break;
            }

            return utrata;
        }
        static int VypocitejUtratuIng(string ing)
        {
            int utrata = 0;
            switch (ing)
            {
                case "salami":
                    utrata = 3;
                    break;
                case "cheese":
                    utrata = 5;
                    break;
                case "salad":
                    utrata = 3;
                    break;
                case "stripsy":
                    utrata = 2;
                    break;
                case "tomatoes":
                    utrata = 1;
                    break;
                case "cucumber":
                    utrata = 2;
                    break;
                case "butter":
                    utrata = 5;
                    break;
                case "ham":
                    utrata = 5;
                    break;
                case "eggs":
                    utrata = 5;
                    break;
                default:
                    utrata = 0;
                    break;
            }
            return utrata;
        }

        public Form1()
        {
            InitializeComponent();
        }

        // Zpracuje textový vstup
        private void EnterTheInput(object sender, EventArgs e)
        {
            string input;
            input = textBox1.Text;
            PrijmiOdpovedAOdpovez(input);
            textBox1.Text = "";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            recEngine.SetInputToDefaultAudioDevice();
            recEngine.SpeechRecognized += recEngine_SpeechRecognised;
        }

        //Hlasovy vstup se zkomprimuje do textove podoby
        void recEngine_SpeechRecognised(object sender, SpeechRecognizedEventArgs e)
        {
            PrijmiOdpovedAOdpovez(e.Result.Text);
        }
        // Po kliknutí na "Enable Voice" se začne nahrávat
        private void BtnEnable_Click(object sender, EventArgs e)
        {
            recEngine.RecognizeAsync(RecognizeMode.Multiple);
            BtnDisable.Enabled = true;
        }
        // Po kliknutí na "Disable Voice" se přestane nahrávat
        private void BtnDisable_Click(object sender, EventArgs e)
        {
            recEngine.RecognizeAsyncStop();
            BtnDisable.Enabled = false;
        }
    }
}
