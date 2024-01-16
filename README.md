# Sterlet
[Sterlet](https://en.wikipedia.org/wiki/Sterlet) is my own home-made chess engine, and GUI chess app developed using C# and Windows Presentation Foundation with Visual Studio 2019.
## Usage
When opened a user can play with the engine or with another human player. Before starting a game you can set up chess clocks and choose the color you want to play if playing versus Sterlet.
Pieces can be moved with drag and drop, the moves available to you with a piece will highlight when that piece is clicked.
## Features
Sterlet's basis is a min-max algorithm with alpha-beta pruning with several optimisations which are detailed below, all of the information about them can be found on [Chess Programming Wiki](https://www.chessprogramming.org/Main_Page).
- **Transpositions table using Zobrist hashing** Efficiently stores board positions using Zobrist hashing for faster move lookups.
- **Opening book with more than 100 000 entries** All of the games were downloaded from [This Week In Chess](https://theweekinchess.com/) and parsed through using my custom made [PGN parser](https://github.com/s3r10us3r/PGNParser). Using opening book allows the engine to be non deterministic since opening lines are being chosen by random.
- **Move ordering with history heuristic** Optimizes move slection.
- **Iterative deepening** allows the engine to dynamically allocate time during gameplay to optimize decision making.
- **Move generation with bitboards** made by myself, it is pretty fast but nowhere near the fastest chess move generators out there.

## Playing strength
Sterlet is currently at around 2000+ in [chess.com](chess.com) ratings. It was tested against Comodo engine available on the site.

## Instalation
You can compile the code on your machine using Visual Studio.

## Future plans
I am not going to expand this project anymore, but I will definitely make a Sterlet 2.0 some time in the future. The topic is wildly fascinating and there aren't many things more satisfying than seeing your own creation beating you up at chess.
## Screenshots
![image](https://github.com/s3r10us3r/Sterlet/assets/116948957/0762b8ce-05f3-4882-acbc-5a71abe64d6c)

![image](https://github.com/s3r10us3r/Sterlet/assets/116948957/9ec2002b-0322-4c68-ac98-627d7af954fe)

![image](https://github.com/s3r10us3r/Sterlet/assets/116948957/e48c878b-a060-470a-b878-020dce6c13fc)

## License
This project is licensed under the [MIT License](LICENSE) - see the [LICENSE](LICENSE) file for details
