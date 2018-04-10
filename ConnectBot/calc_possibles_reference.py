
# establish 7 column 6 row board of zeros
# increment each spot in the board for any potential 
# 4 scoring that it participates in
board = []

for c in range(7):
    board.append([])
    
for c in range(7):
    for r in range(6):
        board[c].append(0)
        
# count straight horizontal scores
for c in range(4):
    for r in range(6):
        for t in range(4):
            board[c + t][r] += 1
            
# count straight vertical scores
for c in range(7):
    for r in range(3):
        for t in range(4):
            board[c][r + t] += 1

# count up right angled scores
for c in range(4):
    for r in range(3):
        for t in range(4):
            board[c + t][r + t] += 1
            
# count up left angled scores
for c in range(3, 7):
    for r in range(3):
        for t in range(4):
            board[c - t][r + t] += 1
      
# print out the board in a nice way and write to a file
s = ""

for r in range(5, -1, -1):
    s += "|"
    
    for c in range(7):
        s += "{0:>3}|".format(board[c][r])
        
    s += "\n"
     
#print(s)
    
with open("board_scores_grid.txt", "w") as fp:
    fp.write(s)
