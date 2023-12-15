using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace Poker
{
    class Program
    {
        // -----------------------
        // DECLARATION DES DONNEES
        // -----------------------
        // Importation des DL (librairies de code) permettant de gérer les couleurs en mode console
        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleTextAttribute(IntPtr hConsoleOutput, int wAttributes);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetStdHandle(uint nStdHandle);
        static uint STD_OUTPUT_HANDLE = 0xfffffff5;
        static IntPtr hConsole = GetStdHandle(STD_OUTPUT_HANDLE);
        // Pour utiliser la fonction C 'getchar()' : sasie d'un caractère
        [DllImport("msvcrt")]
        static extern int _getche();

        //-------------------
        // TYPES DE DONNEES
        //-------------------

        // Fin du jeu
        public static bool fin = false;

        // Codes COULEUR
        public enum couleur { VERT = 10, ROUGE = 12, JAUNE = 14, BLANC = 15, NOIRE = 0, ROUGESURBLANC = 252, NOIRESURBLANC = 240 };

        // Coordonnées pour l'affichage
        public struct coordonnees
        {
            public int x;
            public int y;
        }

        // Une carte
        public struct carte
        {
            public char valeur;
            public int famille;
        };public static Random rnd=new Random();

        // Liste des combinaisons possibles
        public enum combinaison { RIEN, PAIRE, DOUBLE_PAIRE, BRELAN, QUINTE, FULL, COULEUR, CARRE, QUINTE_FLUSH };

        // Valeurs des cartes : As, Roi,...
        public static char[] valeurs = { 'A', 'R', 'D', 'V', 'X', '9', '8', '7', '6', '5', '4', '3', '2' };

        // Codes ASCII (3 : coeur, 4 : carreau, 5 : trèfle, 6 : pique)
        public static int[] familles = { 3, 4, 5, 6 };

        // Numéros des cartes à échanger
        public static int[] echange = { 0, 0, 0, 0 };

        // Jeu de 5 cartes
        public static carte[] MonJeu = new carte[5];

        //----------
        // FONCTIONS
        //----------

        // Génère aléatoirement une carte : {valeur;famille}
        // Retourne une expression de type "structure carte"
        public static carte tirage()
        {
        	int indiceValeur = rnd.Next(0, valeurs.Length);
        	int indiceFamille = rnd.Next(0, familles.Length);
        	
        	carte nouvelleCarte = new carte
        	{
        		valeur = valeurs[indiceValeur],
        		famille = familles[indiceFamille],
        	};
            
        }

        // Indique si une carte est déjà présente dans le jeu
        // Paramètres : une carte, le jeu 5 cartes, le numéro de la carte dans le jeu
        // Retourne un entier (booléen)
        public static bool carteUnique(carte uneCarte, carte[] unJeu, int numero)
        {
        	for (int i = 0; i < numero; i++)
    		{
        		if (unJeu[i].valeur == uneCarte.valeur && unJeu[i].famille == uneCarte.famille)
        			{
            			return false;
        			}
    		}
    		return true;

        }

        // Calcule et retourne la COMBINAISON (paire, double-paire... , quinte-flush)
        // pour un jeu complet de 5 cartes.
        // La valeur retournée est un élement de l'énumération 'combinaison' (=constante)
        public static combinaison chercheCombinaison(carte[] unJeu)
        {
        	 Array.Sort(unJeu, (x, y) => y.valeur.CompareTo(x.valeur)); // Trie les cartes par valeur décroissante

    		// Combinaison : Quinte Flush
    		bool hasQuinteFlush = unJeu[0].valeur - unJeu[4].valeur == 4 && unJeu[0].famille == unJeu[1].famille && unJeu[1].famille == unJeu[2].famille && unJeu[2].famille == unJeu[3].famille && unJeu[3].famille == unJeu[4].famille;

    		// Combinaison : Carré, Full, Couleur, Suite, Brelan, Double Paire, Paire
    		combinaison result = combinaison.RIEN;
    		for (int i = 0; i < unJeu.Length - 1; i++)
    		{
        		int count = 1;
        		for (int j = i + 1; j < unJeu.Length; j++)
        		{
            		if (unJeu[i].valeur == unJeu[j].valeur)
            		{
                		count++;
            		}
        		}

        		if (count == 4)
        		{
            		return combinaison.CARRE;
        		}
        		else if (count == 3)
        		{
            		if (result == combinaison.PAIRE)
            		{
                		result = combinaison.FULL;
            		}
            		else
            		{
                		result = combinaison.BRELAN;
            		}
        		}
        		else if (count == 2)
        		{
            		if (result == combinaison.PAIRE)
            		{
                		result = combinaison.DOUBLE_PAIRE;
            		}
            		else if (result == combinaison.BRELAN)
            		{
                		result = combinaison.FULL;
            		}
            		else
            		{
                		result = combinaison.PAIRE;
            		}
        		}
    		}

    		// Combinaisons finales
    		if (hasQuinteFlush)
    		{
        		return combinaison.QUINTE_FLUSH;
    		}
    		else if (result != combinaison.RIEN)
    		{
        		return result;
    		}
    		else if (unJeu[0].valeur - unJeu[4].valeur == 4)
    		{
        		return combinaison.QUINTE;
    		}
    		else if (unJeu[0].famille == unJeu[1].famille && unJeu[1].famille == unJeu[2].famille && unJeu[2].famille == unJeu[3].famille && unJeu[3].famille == unJeu[4].famille)
    		{
        		return combinaison.COULEUR;
    		}
    		else
    		{
        		return combinaison.RIEN;
    		}
        	

        }

        // Echange des cartes
        // Paramètres : le tableau de 5 cartes et le tableau des numéros des cartes à échanger
		private static void echangeCarte(carte[] unJeu, int[] e)
		{
    		Console.WriteLine("\nEntrez les numéros des cartes que vous souhaitez échanger (1 à 5, séparés par des espaces) :");
    		string input = Console.ReadLine();
    		string[] tokens = input.Split(' ');

    		for (int i = 0; i < e.Length; i++)
    			{
        			if (int.TryParse(tokens[i], out e[i]) && e[i] >= 1 && e[i] <= 5)
        			{
            			e[i]--; // ajuster pour l'indice du tableau
        			}
        			else
        		{
            	Console.WriteLine("Numéro de carte invalide. Veuillez entrer des numéros valides.");
            	echangeCarte(unJeu, e); // redemande les numéros d'échange
            	return;
        		}
    	}

    // Échanger les cartes sélectionnées
    for (int i = 0; i < e.Length; i++)
    {
        unJeu[e[i]] = tirage();
    }
}

        // Pour afficher le Menu pricipale
        private static void afficheMenu()
        {

        }

        // Jouer au Poker
		// Ici que vous appellez toutes les fonction permettant de joueur au poker
        private static void jouerAuPoker()
        {
        	carte[] MonJeu = new carte[5];
    		int[] cartesAEchanger = new int[5];

    		tirageDuJeu(MonJeu);
   			affichageCarte(MonJeu);

    		Console.WriteLine("\nVoulez-vous échanger des cartes ? (O/N)");
    		char reponse = char.ToUpper(Console.ReadKey().KeyChar);

    		if (reponse == 'O')
    			{
        			echangeCarte(MonJeu, cartesAEchanger);
        			affichageCarte(MonJeu);
    			}

    		afficheResultat(MonJeu);
    		enregistrerJeu(MonJeu);

        }

        // Tirage d'un jeu de 5 cartes
        // Paramètre : le tableau de 5 cartes à remplir
        private static void tirageDuJeu(carte[] unJeu)
        { 
        	for (int i = 0; i < 5; i++)
    		{
        		do
        		{
            		unJeu[i] = tirage();
        		} 	
        		while (!carteUnique(unJeu[i], unJeu, i));
    		}

        }

        // Affiche à l'écran une carte {valeur;famille} 
        private static void affichageCarte()
        {
            //----------------------------
            // TIRAGE D'UN JEU DE 5 CARTES
            //----------------------------
            int left = 0;
            int c = 1;
            // Tirage aléatoire de 5 cartes
            for (int i = 0; i < 5; i++)
            {
                // Tirage de la carte n°i (le jeu doit être sans doublons !)

                // Affichage de la carte
                if (MonJeu[i].famille == 3 || MonJeu[i].famille == 4)
                    SetConsoleTextAttribute(hConsole, 252);
                else
                    SetConsoleTextAttribute(hConsole, 240);
                Console.SetCursorPosition(left, 5);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '*', '-', '-', '-', '-', '-', '-', '-', '-', '-', '*');
                Console.SetCursorPosition(left, 6);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', (char)MonJeu[i].famille, ' ', (char)MonJeu[i].famille, ' ', (char)MonJeu[i].famille, ' ', (char)MonJeu[i].famille, ' ', (char)MonJeu[i].famille, '|');
                Console.SetCursorPosition(left, 7);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '|');
                Console.SetCursorPosition(left, 8);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', (char)MonJeu[i].famille, ' ', ' ', ' ', ' ', ' ', ' ', ' ', (char)MonJeu[i].famille, '|');
                Console.SetCursorPosition(left, 9);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', ' ', ' ', ' ', (char)MonJeu[i].valeur, (char)MonJeu[i].valeur, (char)MonJeu[i].valeur, ' ', ' ', ' ', '|');
                Console.SetCursorPosition(left, 10);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', (char)MonJeu[i].famille, ' ', ' ', (char)MonJeu[i].valeur, (char)MonJeu[i].valeur, (char)MonJeu[i].valeur, ' ', ' ', (char)MonJeu[i].famille, '|');
                Console.SetCursorPosition(left, 11);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', ' ', ' ', ' ', (char)MonJeu[i].valeur, (char)MonJeu[i].valeur, (char)MonJeu[i].valeur, ' ', ' ', ' ', '|');
                Console.SetCursorPosition(left, 12);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', (char)MonJeu[i].famille, ' ', ' ', ' ', ' ', ' ', ' ', ' ', (char)MonJeu[i].famille, '|');
                Console.SetCursorPosition(left, 13);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '|');
                Console.SetCursorPosition(left, 14);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', (char)MonJeu[i].famille, ' ', (char)MonJeu[i].famille, ' ', (char)MonJeu[i].famille, ' ', (char)MonJeu[i].famille, ' ', (char)MonJeu[i].famille, '|');
                Console.SetCursorPosition(left, 15);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '*', '-', '-', '-', '-', '-', '-', '-', '-', '-', '*');
                Console.SetCursorPosition(left, 16);
                SetConsoleTextAttribute(hConsole, 10);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", ' ', ' ', ' ', ' ', ' ', c, ' ', ' ', ' ', ' ', ' ');
                left = left + 15;
                c++;
            }

        }

        // Enregistre le score dans le txt
        private static void enregistrerJeu(carte[] unJeu)
		{
    		string scoresFilePath = "scores.txt";

    		// Assurez-vous que le fichier existe avant d'essayer d'écrire dedans
    		if (File.Exists(scoresFilePath))
    		{
        		using (StreamWriter writer = new StreamWriter(scoresFilePath, true))
        		{
            		string scoreLine = "{GetFormattedTimestamp()}: {chercheCombinaison(unJeu)}";
            		writer.WriteLine(scoreLine);
        		}

        		Console.WriteLine("Score enregistré avec succès.");
    		}
    		else
    		{
       			Console.WriteLine("Impossible d'enregistrer le score. Fichier introuvable.");
    		}
		}

		private static string GetFormattedTimestamp()
		{
    		// Retourne un horodatage simple sous forme de chaîne
    		return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
		}


        // Affiche le Scores
        private static void voirScores()
        {
        	string scoresFilePath = "scores.txt";

    		if (File.Exists(scoresFilePath))
    		{
        		string[] scores = File.ReadAllLines(scoresFilePath);

        		if (scores.Length > 0)
        		{
            		Console.WriteLine("Scores enregistrés :");

            		foreach (string score in scores)
            		{
                		Console.WriteLine(score);
            		}
        		}
        		else
        		{
            		Console.WriteLine("Aucun score enregistré.");
        		}
    		}
   			else
    		{
        		Console.WriteLine("Aucun score enregistré.");
    		}
        }

        // Affiche résultat
        private static void afficheResultat(carte[] unJeu)
        {
            SetConsoleTextAttribute(hConsole, 012);
            Console.Write("RESULTAT - Vous avez : ");
            try
            {
                // Test de la combinaison
                switch (chercheCombinaison(ref MonJeu))
                {
                    case combinaison.RIEN:
                        Console.WriteLine("rien du tout... desole!"); break;
                    case combinaison.PAIRE:
                        Console.WriteLine("une simple paire..."); break;
                    case combinaison.DOUBLE_PAIRE:
                        Console.WriteLine("une double paire; on peut esperer..."); break;
                    case combinaison.BRELAN:
                        Console.WriteLine("un brelan; pas mal..."); break;
                    case combinaison.QUINTE:
                        Console.WriteLine("une quinte; bien!"); break;
                    case combinaison.FULL:
                        Console.WriteLine("un full; ouahh!"); break;
                    case combinaison.COULEUR:
                        Console.WriteLine("une couleur; bravo!"); break;
                    case combinaison.CARRE:
                        Console.WriteLine("un carre; champion!"); break;
                    case combinaison.QUINTE_FLUSH:
                        Console.WriteLine("une quinte-flush; royal!"); break;
                };
            }
            catch { }
        }


        //--------------------
        // Fonction PRINCIPALE
        //--------------------
        static void Main(string[] args)
        {
            //---------------
            // BOUCLE DU JEU
            //---------------
            char reponse;
            while (true)
            {
                afficheMenu();
                reponse = (char)_getche();
                if (reponse != '1' && reponse != '2' && reponse != '3')
                {
                    Console.Clear();
                    afficheMenu();
                }
                else
                {
                SetConsoleTextAttribute(hConsole, 015);
                // Jouer au Poker
                if (reponse == '1')
                {
                    int i = 0;
                    jouerAuPoker();
                }

                if (reponse == '2')
                    voirScores();

                if (reponse == '3')
                    break;
            }
            }
            Console.Clear();
        }
    }
}



