/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Collections.Generic;
using ClassicUno.Classes.Settings.SettingsData;

namespace ClassicUno.Classes.Helpers
{
    public static class ComputerNames
    {
        /* Simple class for generating random Computer player names and genders */
        private static readonly Random Rng = new Random();

        private static char RandomLetter() => (char)('A' + Rng.Next(26));
        
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
            "Mick"
        };

        private static readonly List<string> FemaleNames = new List<string>()
        {
            "Samantha",
            "Diane",
            "Julie",
            "Annie",
            "Amy",
            "Jenny",
            "Alica",
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
            "Toni",
            "Ivy",
            "Kath"
        };

        public static PlayerNameData GetRandomName()
        {
            PlayerGender gender;
            List<string> names;
            /* Choose a random gender */
            switch (Rng.Next(0, 2))
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
            /* Pick a random name based on gender */
            var name = $"{names[Rng.Next(0, names.Count - 1)]} {RandomLetter()}.";
            var data = new PlayerNameData
            {
                Name = name,
                Gender = gender
            };
            return data;
        }
    }
}
