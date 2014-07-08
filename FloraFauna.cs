using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntoTheNewWorld
{
    public class FloraFauna
    {
        // Jungle: jaguar, piranha (river), howler monkey, anaconda, capybara
        // Temperate rain forest: sequoia
        // Forest: black bear, cougar, bobcat, bald eagle (near water), turkey, opossum, armadillo (near river), raccoon, skunk, live oak, beaver (near water), nutria (near water)
        // Evergreen Forest: mink
        // Tundra: polar bear 
        // Hills: grizzly bear (non-tropical), rattlesnake, coyote
        // Plains: buffalo, prarie dog, groundhog
        // Swamp: alligator, spanish moss, muskrat, river otter
        // Mountains: llama, alpaca, mountain goat, condor, chinchilla, bighorn sheep
        // Desert: cougar, roadrunner, cactus, joshua tree
        // Foods: beans (lima, pinto, kidney), peppers (bell, chili), nuts (cashew, peanut, pecan), gourds (squash, pumpkin, zucchini),
        //        berries (blackberry, blueberry, cranberry, strawberry), maize, potato, sweet potato, tomato, cocoa, vanilla, pineapple,
        //        tobacco, sunflower, rubber
        // Legendary: Sasquatch, Jersey Devil, Fountain of Youth, El Dorado, Chupacabra, Champ, Kingdom of Saguenay, Northwest Passage,
        //            Atlantis, Paititi, La Canela, Sierra de la Plata, Quivira, Cibola, City of the Caesars, Vinland, Lost Colony (?)
        // Man-made: Cahokia mounds, Chichen Itza, Machu Picchu (?), Viking ruins, Mesa Verde.

        public string identifier { get; private set; }
        public Terrain.TerrainMinorType habitat { get; private set; }
        public bool nearWater { get; private set; }
        public int discoveryChance { get; private set; }

        public FloraFauna(string identifier, Terrain.TerrainMinorType habitat, bool nearWater, int discoveryChance)
        {
            this.identifier = identifier;
            this.habitat = habitat;
            this.nearWater = nearWater;
            this.discoveryChance = discoveryChance;
        }
    }
}
