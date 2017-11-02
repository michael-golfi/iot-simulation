using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WMT
{
    class Program
    {
        static void Main(string[] args)
        {
            int inputNumOfFridges;
            double inputEstimatedValueInFridge;
            string inputInsideTemperature;
            string inputIsDoorOpen;
            string inputSealThreshold;
            string inputSealBroken = "0";           // Hardcoded for Azure ML Services
            Tuple<int, double> resultTuple;         // Results from Azure ML Services
            int scoredLabel;                        // Tuple Item 1
            double scoredProbability;               // Tuple Item 2
            bool rerunProgramFlag;

            do
            {
                // Method calls for user input
                inputNumOfFridges = Convert.ToInt32(Math.Floor(Convert.ToDouble(GetNumberInput("number of refrigerators")))); // Convert to string to int
                inputEstimatedValueInFridge = Convert.ToDouble(GetNumberInput("estimated value in fridge")); // Convert string to double
                inputInsideTemperature = GetNumberInput("inside temperature");
                inputIsDoorOpen = GetStateOfDoorInput();
                inputSealThreshold = GetNumberInput("seal threshold");

                // Task call for HTTP request to Azure ML Services. Returns Tuple<int, double> as Tuple<Scored Label, Scored Probability>
                resultTuple = InvokeRequestResponseService(inputInsideTemperature, inputIsDoorOpen, inputSealThreshold, inputSealBroken).Result;
                
                if (resultTuple != null) // Successful HTTP Request
                {
                    scoredLabel = resultTuple.Item1;
                    scoredProbability = resultTuple.Item2;

                    // Method call to print out results and potential loss
                    ResultForecast(inputNumOfFridges, inputEstimatedValueInFridge, scoredLabel, scoredProbability);
                }
                else // Unsuccessful HTTP Request
                {
                    Console.WriteLine("Error occurred in accessing Azure ML Services");
                }

                // Method call to see if the user would like to rerun the program
                rerunProgramFlag = RerunProgramCheck();
                Console.Write("\n\n");

            } while (rerunProgramFlag);
        }


        /*
         * METHOD: Gets user input for state of door. Only accepts 0 (closed door) and 1 (open door) as user input,
         *         otherwise would prompt for correct input.
         * RETURN: String ("0" or "1" only) [Returns as string for Azure ML Services inputs]
         */
        public static string GetStateOfDoorInput()
        {
            string inputLine;
            int stateOfDoor;
            bool correctInputFlag;

            do
            {
                Console.Write("Enter door state (0 or 1): ");
                inputLine = Console.ReadLine();

                // Checks if input is an int
                if (int.TryParse(inputLine, out stateOfDoor))
                {
                    // Checks if input is either 0 or 1 (only accepted inputs)
                    if (stateOfDoor == 0 || stateOfDoor == 1)
                    {
                        correctInputFlag = true;
                        Console.Write("\n");
                    }
                    else
                    {
                        correctInputFlag = false;
                        Console.Write("Only input 0 or 1 is accepted. \n\n");
                    }
                }
                else
                {
                    correctInputFlag = false;
                    Console.Write("Only input 0 or 1 is accepted. \n\n");
                }
            } while (!correctInputFlag);

            return inputLine;
        }


        /*
         * METHOD: Gets user input for numbers (double) for a given input variable. 
         *         If user input is non-numerical, it would prompt for correct input.
         * RETURN: String (numerical (double) values only) [Returns as string for Azure ML Services inputs]
         */
        public static string GetNumberInput(string inputName)
        {
            string inputLine;
            double num;
            bool correctInputFlag;

            do
            {
                Console.Write("Enter {0}: ", inputName);
                inputLine = Console.ReadLine();

                // Check if input is numberical (double)
                if (Double.TryParse(inputLine, out num))
                {
                    correctInputFlag = true;
                    Console.Write("\n");
                }
                else
                {
                    correctInputFlag = false;
                    Console.Write("Number input expected. \n\n");
                }
            } while (!correctInputFlag);

            return inputLine;
        }


        /* 
         * METHOD: Prints out Azure ML Services results and calculates for potential spoilage loss.
         * RETURN: N/A
         */
        public static void ResultForecast (int numOfFridges, double estimatedValueInFridge, int scoredLabel, double scoredProbability)
        {
            double totalLoss = numOfFridges * estimatedValueInFridge;

            Console.WriteLine("===================================================");

            if (scoredLabel == 1) // Refrigerator seal is broken
            {
                Console.WriteLine("\n");
                Console.WriteLine("********************* WARNING *********************");
                Console.WriteLine("========== Refrigerator seals are broken ==========");
                Console.WriteLine("***************************************************");
            }

            Console.WriteLine("\n");
            Console.WriteLine("Probability of broken seal: \t {0}", scoredProbability.ToString("P"));
            Console.WriteLine("Potential Spoilage Loss: \t {0}", totalLoss.ToString("C"));
            Console.WriteLine("\n");
            Console.WriteLine("===================================================");
            Console.WriteLine("\n");
        }


        /*
         * TASK:   Task for HTTP Request to Azure ML Services. 
         *         Provides user input as request input and gets output from Azure ML Services.
         * RETURN: Tuple<int, double> as <Score Label, Score Probability>
         *         Score Label - 0 (seal not broken) or 1 (seal broken)
         *         Score Probability - Probability of broken seal
         */
        static async Task<Tuple<int, double>> InvokeRequestResponseService(string inputInsideTemperature, string inputIsDoorOpen, string inputSealThreshold, string inputSealBroken)
        {
            using (var client = new HttpClient())
            {
                var scoreRequest = new
                {
                    Inputs = new Dictionary<string, List<Dictionary<string, string>>>() {
                        {
                            "input1",
                            new List<Dictionary<string, string>>(){new Dictionary<string, string>(){
                                    {
                                        "InsideTemperature", inputInsideTemperature
                                    },
                                    {
                                        "IsDoorOpen", inputIsDoorOpen
                                    },
                                    {
                                        "SealThreshold", inputSealThreshold
                                    },
                                    {
                                        "SealBroken", inputSealBroken
                                    },
                                }
                            }
                        },
                    },
                    GlobalParameters = new Dictionary<string, string>()
                    {
                    }
                };

                const string apiKey = "pUE8N74+Jjifwb6kY5T8g0bFsSj1Us6tpVUAI216EWsIjsvLIVtSvvmVn788GXOQQFzuRNVxmcYWHKeg5tfqhA== ";
                const string apiUri = "https://ussouthcentral.services.azureml.net/workspaces/e88a1c567cc14277ac69904e0b53ecd8/services/e885ddcb8e1e430bbd5a5fe9e9976d99/execute?api-version=2.0&format=swagger";

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.BaseAddress = new Uri(apiUri);

                // WARNING: The 'await' statement below can result in a deadlock if you are calling this code from the UI thread of an ASP.Net application.
                // One way to address this would be to call ConfigureAwait(false) so that the execution does not attempt to resume on the original context.
                // For instance, replace code such as:
                //      result = await DoSomeTask()
                // with the following:
                //      result = await DoSomeTask().ConfigureAwait(false)

                HttpResponseMessage response = await client.PostAsJsonAsync("", scoreRequest);

                if (response.IsSuccessStatusCode) // Successful HTTP request
                {
                    string result = await response.Content.ReadAsStringAsync(); // Result from Azure ML Services as string in JSON format
                    JObject resultJSON = JObject.Parse(result);

                    string scoredLabel = resultJSON["Results"]["output1"][0]["Scored Labels"].ToString(); // Get Scored Label value as string
                    string scoredprob = resultJSON["Results"]["output1"][0]["Scored Probabilities"].ToString(); // Get Scored Probability as string

                    Tuple<int, double> resultTuple = new Tuple<int, double>(Convert.ToInt32(scoredLabel), Convert.ToDouble(scoredprob));

                    return resultTuple;
                }
                else // Unsuccessful HTTP request
                {
                    Console.WriteLine(string.Format("The request failed with status code: {0}", response.StatusCode));

                    // Print the headers - they include the requert ID and the timestamp, which are useful for debugging the failure
                    Console.WriteLine(response.Headers.ToString());

                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseContent);

                    return null; 
                }
            }
        }
        

        /* 
         * METHOD: Prompts user if they would want to rerun the program. 
         *         Only accepts 'Y' or 'y' for Yes and 'N' or 'n' for No
         * RETURN: bool
         */
        public static bool RerunProgramCheck ()
        {
            string inputLine;

            do
            {
                Console.Write("Re-calculate values (y/n)? ");
                inputLine = Console.ReadLine();

                if (inputLine.ToLower() == "y")
                    return true;
                else if (inputLine.ToLower() == "n")
                    return false;
                else
                    Console.Write("Only 'y' and 'n' accepted. \n\n");

            } while (inputLine != "y" || inputLine != "n");

            return false;
        }

    }
}
