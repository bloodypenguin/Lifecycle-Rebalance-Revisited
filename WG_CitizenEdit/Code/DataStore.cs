namespace WG_CitizenEdit
{
    public class DataStore
    {
        // Citizen life span --------------------------------------------------
        public static int lifeSpanMultiplier = 4;
        public static int workSpeedMultiplier = 2;  // Might as well be half of the multiplier


        // Survival number to next decile. In units of 1/100000
        // Each decile is 25 ticks. There are 5 ticks at the end. Kill the last one
        // Source: http://www.aga.gov.au/publications/life_table_2010-12/ (averaged out with both genders and in blocks of 10 years)
        // Per decile, raw data
        public static double[] survivalProbInXML = { 0.99514, 0.99823, 0.99582, 0.99326, 0.98694, 0.97076, 0.93192, 0.82096, 0.50858, 0.11799, 0.01764 };
        public static int[] survivalProbCalc = new int[survivalProbInXML.Length];

        // Per decile who will get sick
        public static double[] sicknessProbInXML = { 0.0125, 0.0075, 0.01, 0.0125, 0.015, 0.02, 0.025, 0.03, 0.0375, 0.05, 0.5 };
        public static int[] sicknessProbCalc = new int[sicknessProbInXML.Length];

        public static int[] citizenNumberBounds;

        public static int[] incomingAdultAge = {85, 190};

        // Travel -------------------------------------------------------------
        public const int LOW = 0;
        public const int HIGH = 1;

        // Array indexes
        public const int CAR = 0;
        public const int BIKE = 1;
        public const int TAXI = 2;

        // TODO? - Percentage for bike riding (add bike)
        public static int bikeIncrease = 10;

        // wealth, home building density, age, transportmode
        public static int[][][] wealth_low = { new int[][] { new int [] { 0, 40, 0},
                                                             new int [] {10, 30, 0},
                                                             new int [] {50, 20, 1},
                                                             new int [] {65, 10, 2},
                                                             new int [] {35,  0, 3} },

                                               new int[][] { new int [] {0, 40, 0},
                                                             new int [] {2, 30, 0},
                                                             new int [] {3, 20, 1},
                                                             new int [] {4, 10, 2},
                                                             new int [] {3,  0, 3} }};

        public static int[][][] wealth_med = { new int[][] { new int [] { 0, 40, 0},
                                                             new int [] {12, 30, 1},
                                                             new int [] {55, 20, 2},
                                                             new int [] {70, 10, 4},
                                                             new int [] {40,  0, 6} },

                                               new int[][] { new int [] {0, 40, 0},
                                                             new int [] {3, 30, 1},
                                                             new int [] {5, 20, 2},
                                                             new int [] {6, 10, 3},
                                                             new int [] {5,  0, 5} }};

        public static int[][][] wealth_high = { new int[][] { new int [] { 0, 40, 0},
                                                              new int [] {15, 30, 2},
                                                              new int [] {60, 20, 3},
                                                              new int [] {75, 10, 4},
                                                              new int [] {50,  0, 6} },

                                                new int[][] { new int [] { 0, 40, 0},
                                                              new int [] { 4, 30, 2},
                                                              new int [] { 6, 20, 3},
                                                              new int [] { 8, 10, 4},
                                                              new int [] { 7,  0, 5} }};
    }
}