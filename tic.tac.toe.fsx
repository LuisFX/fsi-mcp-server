// Tic-Tac-Toe Game in F#

type Player = X | O

type Cell = Empty | Filled of Player

type Board = Cell array array

type GameState =
    | InProgress of Player
    | Won of Player
    | Draw

let createBoard() : Board =
    Array.init 3 (fun _ -> Array.create 3 Empty)

let cellToString cell =
    match cell with
    | Empty -> " "
    | Filled X -> "X"
    | Filled O -> "O"

let displayBoard (board: Board) =
    printfn "\n  1   2   3"
    printfn "┌───┬───┬───┐"
    for i in 0..2 do
        printf "%c " (char (65 + i))
        for j in 0..2 do
            printf "│ %s " (cellToString board.[i].[j])
        printfn "│"
        if i < 2 then
            printfn "├───┼───┼───┤"
    printfn "└───┴───┴───┘"

let parseMove (input: string) =
    if input.Length <> 2 then
        None
    else
        let row = int input.[0] - int 'A'
        let col = int input.[1] - int '1'
        if row >= 0 && row <= 2 && col >= 0 && col <= 2 then
            Some (row, col)
        else
            None

let isValidMove (board: Board) row col =
    board.[row].[col] = Empty

let makeMove (board: Board) row col player =
    let newBoard = Array.map Array.copy board
    newBoard.[row].[col] <- Filled player
    newBoard

let checkLine (board: Board) (positions: (int * int) list) =
    let cells = positions |> List.map (fun (r, c) -> board.[r].[c])
    match cells with
    | [Filled p1; Filled p2; Filled p3] when p1 = p2 && p2 = p3 -> Some p1
    | _ -> None

let checkWinner (board: Board) =
    let lines = [
        // Rows
        [(0,0); (0,1); (0,2)]
        [(1,0); (1,1); (1,2)]
        [(2,0); (2,1); (2,2)]
        // Columns
        [(0,0); (1,0); (2,0)]
        [(0,1); (1,1); (2,1)]
        [(0,2); (1,2); (2,2)]
        // Diagonals
        [(0,0); (1,1); (2,2)]
        [(0,2); (1,1); (2,0)]
    ]

    lines
    |> List.tryPick (checkLine board)

let isBoardFull (board: Board) =
    board |> Array.forall (Array.forall ((<>) Empty))

let getGameState (board: Board) currentPlayer =
    match checkWinner board with
    | Some winner -> Won winner
    | None when isBoardFull board -> Draw
    | None -> InProgress currentPlayer

let switchPlayer player =
    match player with
    | X -> O
    | O -> X

let rec gameLoop board currentPlayer =
    displayBoard board

    let gameState = getGameState board currentPlayer
    match gameState with
    | Won winner ->
        printfn "\n🎉 Player %A wins! 🎉" winner
    | Draw ->
        printfn "\n🤝 It's a draw! 🤝"
    | InProgress player ->
        printfn "\nPlayer %A's turn" player
        printf "Enter move (e.g., A1, B2, C3): "
        let input = System.Console.ReadLine()

        if input.ToLower() = "quit" then
            printfn "Thanks for playing!"
        else
            match parseMove (input.ToUpper()) with
            | Some (row, col) ->
                if isValidMove board row col then
                    let newBoard = makeMove board row col player
                    let nextPlayer = switchPlayer player
                    gameLoop newBoard nextPlayer
                else
                    printfn "❌ That position is already taken!"
                    gameLoop board player
            | None ->
                printfn "❌ Invalid move format! Use format like A1, B2, C3"
                gameLoop board player

let playGame() =
    printfn "Welcome to Tic-Tac-Toe!"
    printfn "========================"
    printfn "Enter moves using row (A-C) and column (1-3), e.g., A1, B2, C3"
    printfn "Type 'quit' to exit the game\n"

    let board = createBoard()
    gameLoop board X

// Run the game
playGame()