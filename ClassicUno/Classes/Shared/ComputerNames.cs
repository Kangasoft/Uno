/* Classic Uno 2026
 * Version: 1.0.1
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System.Collections.Generic;
using ClassicUno.Classes.Settings.SettingsData;

namespace ClassicUno.Classes.Shared
{
    public static class ComputerNames
    {
        /* Simple class for generating random Computer player names and genders */
        private static char RandomLetter() => (char)('A' + RandomEx.Shared.Next(23)); /* Stop at W */

        private static readonly List<string> MaleNames = new List<string>()
        {
            "John",
            "Tim",
            "Sam",
            "Bill",
            "Henry",
            "Paul",
            "Michael",
            "Mitch",
            "Steve",
            "William",
            "Martin",
            "Colin",
            "Allen",
            "Tom",
            "Jim",
            "Rob",
            "Peter",
            "Simon",
            "Dan",
            "Tony",
            "Max",
            "Mick",
            "Ken",
            "Ryan",
            "Harry"
        };

        private static readonly List<string> FemaleNames = new List<string>()
        {
            "Samantha",
            "Diane",
            "Julie",
            "Annie",
            "Amy",
            "Jenny",
            "Alicia",
            "Jasmine",
            "Wendy",
            "Rebecca",
            "Claire",
            "Amanda",
            "Jessica",
            "Chloe",
            "Kendra",
            "Jane",
            "Judy",
            "Anna",
            "Sandy",
            "Rose",
            "Ivy",
            "Kath",
            "Karen",
            "Amber",
            "Madison"
        };

        public static PlayerNameData GetRandomName()
        {
            PlayerGender gender;
            List<string> names;
            /* Choose a random gender */
            switch (RandomEx.Shared.Next(0, 2))
            {
                case 0:
                    gender = PlayerGender.Male;
                    names = MaleNames;
                    break;

                default:
                    gender = PlayerGender.Female;
                    names = FemaleNames;
                    break;
            }
            /* Pick a random name based on gender.
             * Corrected names.Count -1 to just name.Count to not exclude last name of either list.
             */
            var name = $"{names[RandomEx.Shared.Next(names.Count)]} {RandomLetter()}.";
            var data = new PlayerNameData
            {
                Name = name,
                Gender = gender
            };
            return data;
        }
    }
}
