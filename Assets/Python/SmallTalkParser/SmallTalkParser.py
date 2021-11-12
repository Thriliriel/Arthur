#default return path
rpath = "../../../"

#Get the dialogues
f = open(rpath+"newDialogues.txt", "r")
lines = f.readlines()
f.close()

#get ids
f = open("stIds.txt", "r")
ids = f.readlines()
f.close()

nextTopic = int(ids[0])
nextDialog = int(ids[1])

dialogues = []
sentences = []

for line in lines:
    if(not (line and line.strip())):
        #we pass sentences[:], so we can clear the list without losing it in dialogues (since it just references)
        dialogues.append(sentences[:])
        sentences.clear()
        continue

    striped = line.strip()
    #print("Line{}: {}".format(count, striped))
    sentences.append(striped)

#add the last one
dialogues.append(sentences[:])
sentences.clear()

#for line in dialogues:
#    print(line)

#get the number of iterations
ni = len(dialogues[0])

#write files
f = open(rpath+"smallTalk.txt", "a")
hist = open(rpath+"AutobiographicalStorage/historic.txt", "a")

dialogTreeId = 1
dialogTreeLevel = 1

#if we have more than 1 dialog, we check it all
if len(dialogues) > 1:
    #write headers
    f.write("\n$ newTopic"+str(nextTopic)+"\n")
    f.write("[ newDialog"+str(nextDialog)+"\n")

    differentIndexes = []

    for i in range(ni):
        #we just need to check the even indexes, where we find what A speaks
        if i % 2 != 0: 
            continue

        #we iterate through the dialogues, checking the same indexes to seek for differences
        #always compare with the first one (0)
        equality = True
        for index,dial in enumerate(dialogues):
            if index == 0:
                continue

            #if it is different
            if dialogues[0][i] != dial[i]:
                print(dialogues[0][i] + " - " + dial[i])
                equality = False

        #if it is not equal, we store this index to check later
        if not equality:
            differentIndexes.append(i)

    #now that we checked which are different, we start to save
    parentId = "-1"
    answerHistoric = ""

    for index,sentence in enumerate(dialogues[0]):
        #split by :
        info = sentence.split(':')

        #if the index is not present in different, all good

        if index not in differentIndexes:
            #if it is A, it is for the agent
            #if it is B, it is answer
            if info[0] == "A":
                dtid = "0" + str(dialogTreeId)
                if dialogTreeId >= 10: 
                    dtid = str(dialogTreeId)

                dtl = "0" + str(dialogTreeLevel)
                if dialogTreeLevel >= 10: 
                    dtl = str(dialogTreeLevel)

                stId = "RT" + str(nextTopic) + "d" + str(nextDialog) + dtid + dtl;
                f.write("# " + stId + "; " + info[1].strip() + "; "+parentId+";\n")

                #if answerHistoric is not empty, it means we can keep historic
                if answerHistoric != "":
                    hist.write(parentId + ";" + answerHistoric + ";" + stId + "\n")

                parentId = stId

                dialogTreeId += 1
                dialogTreeLevel += 1
            elif info[0] == "B":
                answerHistoric = info[1].strip()
        #otherwise, it means we need to branch
        else:
            #thus, for each dialog, we get its sentence and save with the last answer
            stId = 0
            for dial in dialogues:
                diffInfo = dial[index].split(':')

                dtid = "0" + str(dialogTreeId)
                if dialogTreeId >= 10: 
                    dtid = str(dialogTreeId)

                dtl = "0" + str(dialogTreeLevel)
                if dialogTreeLevel >= 10: 
                    dtl = str(dialogTreeLevel)

                stId = "RT" + str(nextTopic) + "d" + str(nextDialog) + dtid + dtl;
                f.write("# " + stId + "; " + diffInfo[1].strip() + "; "+parentId+";\n")

                diffInfoHist = dial[index-1].split(':')
                hist.write(parentId + ";" + diffInfoHist[1].strip() + ";" + stId + "\n")

                dialogTreeId += 1

            dialogTreeLevel += 1
            parentId = stId

    #done
    f.write("]\n")
    nextTopic += 1
    nextDialog += 1

#else, if we have only 1 dialog, it is only one sequence to follow
elif len(dialogues) == 1:
    #write headers
    f.write("$ newTopic"+str(nextTopic)+"\n")
    f.write("[ newDialog"+str(nextDialog)+"\n")

    parentId = "-1"
    answerHistoric = ""

    for sentence in dialogues[0]:
        #split by :
        info = sentence.split(':')

        #if it is A, it is for the agent
        #if it is B, it is answer
        if info[0] == "A":
            dtid = "0" + str(dialogTreeId)
            if dialogTreeId >= 10: 
                dtid = str(dialogTreeId)

            dtl = "0" + str(dialogTreeLevel)
            if dialogTreeLevel >= 10: 
                dtl = str(dialogTreeLevel)

            stId = "RT" + str(nextTopic) + "d" + str(nextDialog) + dtid + dtl;
            f.write("# " + stId + "; " + info[1].strip() + "; "+parentId+";\n")

            #if answerHistoric is not empty, it means we can keep historic
            if answerHistoric != "":
                hist.write(parentId + ";" + answerHistoric + ";" + stId + "\n")

            parentId = stId

            dialogTreeId += 1
            dialogTreeLevel += 1
        elif info[0] == "B":
            answerHistoric = info[1].strip()

    #done
    f.write("]\n")
    nextTopic += 1
    nextDialog += 1
else:
    print("No dialogs found!")

f.close()
hist.close()

f = open("stIds.txt", "w+")
f.write(str(nextTopic) + "\n")
f.write(str(nextDialog) + "\n")
f.close()