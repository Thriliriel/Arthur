'''

    Willian S. Dias, 2020

    Source(s):
    https://medium.com/@paulo_sampaio/entendendo-k-means-agrupando-dados-e-tirando-camisas-e90ae3157c17
    https://jakevdp.github.io/PythonDataScienceHandbook/05.12-gaussian-mixtures.html

'''

from sklearn.cluster import KMeans
from sklearn.mixture import GaussianMixture
import matplotlib.pyplot as plt
import matplotlib.image as img
import pandas as pd
import shutil
import sys
import os

cwd = ""

# ---------------------------------------------------------------------------------------------------- #

def turnBack(user, emotion, picture, originalPath):

    # Defines clustering (final) output directory
    clusterDir = originalPath + "\\Clustering\\Profiles"

    # Create user cluster "profile" directory
    if not os.path.isdir(clusterDir):
        os.mkdir(clusterDir)

    # Defines cluster input data filename
    filename = "clusters_" + picture.replace("img.png", emotion + ".csv")

    # Reads and formats input file data
    data = pd.read_csv(cwd + "\\Clusters\\" + filename)
    data = data[data['face'].str.contains(user)]
    data['face'].replace(to_replace = picture.split("_")[0], value = emotion, inplace = True)
    
    # Defines ouput file name
    userFile = clusterDir + "\\" + user + ".csv"

    # If "profile" already exists, replaces or appends new data
    if os.path.isfile(userFile):

        # Reads existing "profile"
        userData = pd.read_csv(userFile)

        # Replaces existing (emotion) row or add new one
        if (userData['face'] == emotion).any():
            userData.loc[userData["face"] == emotion] = data.values
        else:
            userData = userData.append(data)
        
        # Replaces old (data) object, in order to write
        # updated or new emotion clusters
        data = userData

    # Write changes
    data.to_csv(userFile, index = False)

# ---------------------------------------------------------------------------------------------------- #

def clustering(emotion, picture):

    # Considering the number of different animations per emotion
    # (not the number of clusters from article, where K-Means != GMM)
    nClusters = {
                    "Happiness": { "K-Means": 3, "GMM": 3 },
                    "Sadness": { "K-Means": 4, "GMM": 4 },
                    "Surprise": { "K-Means": 3, "GMM": 3 },
                    "Anger": { "K-Means": 3, "GMM": 3 },
                    "Disgust": { "K-Means": 3, "GMM": 3 },
                    "Fear": { "K-Means": 4, "GMM": 4 }
                }

    probs = []

    # Gets input file name, emotion and number of clusters
    file = picture.replace(".png", "") + "_" + emotion + "_output.csv"
    filename = file.split("_")[2].title()
    nck = nClusters[filename]["K-Means"]
    ncg = nClusters[filename]["GMM"]
    
    # Reads data and selects only AUs related
    data = pd.read_csv(cwd + "\\CSV\\" + file)
    data_values = []
    for values in data.values:
        data_values.append(values[2:])

    # Creates clustering objects
    kmeans = KMeans(n_clusters = nck)
    gmm = GaussianMixture(n_components = ncg)

    # Does clustering
    data["cluster_KMeans"] = kmeans.fit_predict(data_values)
    data["cluster_GMM"] = gmm.fit_predict(data_values)

    # Calculates probabilities of belonging to a certain cluster (GMM only)
    probs = gmm.predict_proba(data_values)

    # Groups all for further writing
    cluster = (file, data, pd.DataFrame(probs))

    # Defines output directory
    directory = cwd + "\\Clusters"

    # Creates clusters' directory (useful for first run only)
    if not os.path.isdir(directory):
        os.mkdir(directory)

    # Filters subject and respective cluster (K-Means & GMM)
    data = cluster[1].filter(["face", "cluster_KMeans", "cluster_GMM"])

    # Gets output filename (partial)
    file = cluster[0].replace("_img", "").replace("_output.csv", "")

    # Gets first column (header and data)
    probs = cluster[1].filter(["face"])

    # Gets probabilities of each subject to belong to a certain cluster
    for item in cluster[2]:
        probs[item] = cluster[2][item]

    # Writes clusters
    data.to_csv(directory + "\\clusters_" + file + ".csv", index = False)

    # Writes probabilities
    probs.to_csv(directory + "\\gmm_probs_" + file + ".csv", index = False)

# ---------------------------------------------------------------------------------------------------- #

def group(emotion, picture):

    # Relation between file numbering and emotion
    relations = {
                    "01": "Happiness", "02": "Sadness", "03": "Surprise",
                    "04": "Anger", "05": "Disgust", "06": "Fear"
                }

    # Directories where input data come from
    inputData = "OpenFace_Output"
    dsInputData = inputData + "\\Dataset_Output"

    # Directory output data (grouping)
    directory = cwd + "\\CSV"

    # Creates output directory (useful for first run only)
    if not os.path.isdir(directory):
        os.mkdir(directory)

    # Find Nº that matches current emotion
    for key in relations:
        if relations[key] == emotion:
            item = key

    header = ""
    lines = []

    # For each CSV file in dataset input directory
    for file in os.listdir(cwd + "\\" + dsInputData):
        if file.endswith(".csv"):

            # Verifies if filename matches emotion numbering
            if file.split("-")[1].split("_")[0] == item:
                pointer = open(cwd + "\\" + dsInputData + "\\" + file, "r")
                content = pointer.readlines()

                # Keeps only face, action unit columns from header
                headerContent = content[0].split(",")[:18]
                # Keeps only data from action units
                bodyContent = content[1].split(",")[:18]

                # Groups header and body again
                header = ",".join(headerContent) + "\n"
                body = ",".join(bodyContent) + "\n"
                
                # Identifies subject by filename
                subject = file.split("_")[0]
                data = body[1:]

                # Puts all together into the basket
                line = subject + data
                lines.append(line)

                pointer.close()

    # Does (almost) the same for current user
    file = picture.replace(".png", ".csv")
    pointer = open(cwd + "\\" + inputData + "\\" +  file, "r")
    content = pointer.readlines()

    # Keeps only data from action units
    bodyContent = content[1].split(",")[:18]

    # Groups body again
    body = ",".join(bodyContent) + "\n"
    
    # Identifies subject by filename
    subject = file.split("_")[0]
    data = body[1:]

    # Puts all together into the basket
    line = subject + data
    lines.append(line)

    pointer.close()

    # After reading and grouping everything, writes down to a file
    subject = picture.split(".")[0] + "_" + relations[item]
    outputPath = directory + "\\" + subject + "_output.csv"
    output = open(outputPath, "w")
    output.write(header)
    for line in lines:
        str = ""
        for item in line:
            str += item
        output.write(str)
    output.close()

# ---------------------------------------------------------------------------------------------------- #

def extract(picture, all):

    # Defines where needed stuff is
    binary = cwd + "\\OpenFace\\FaceLandmarkImg.exe"
    images = cwd + "\\Images"
    subjects = images + "\\Dataset"
    output = cwd + "\\OpenFace_Output"
    dataset = output + "\\Dataset_Output"

    aux = {}
    # Creates a dictionary of dataset filenames
    for subject in os.listdir(subjects):
            for image in os.listdir(subjects + "\\" + subject):
                tmp = image.split(".")[0]
                aux[tmp] = ""

    # Verifies if dataset output directory exists
    # (it's important only for the first run)
    if not os.path.isdir(output):
        print("OUTPUT")
        os.mkdir(output)

    if not os.path.isdir(dataset):
        print("DATASET")
        os.mkdir(dataset)

    # Does for all subjects
    if all:

        # Removes all dataset extracted data
        shutil.rmtree(dataset)
        os.mkdir(dataset)

        # For each picture, of each subject, runs data (AU - action unit) extraction
        for subject in os.listdir(subjects):
            for image in os.listdir(subjects + "\\" + subject):
                path = subjects + "\\" + subject + "\\" + image
                command = "\"" + binary + " -aus -f " + path + " -out_dir " + dataset + "\""
                os.system(command)

    # Runs for actual user
    path = images + "\\" + picture
    command = "\"" + binary + " -aus -f " + path + " -out_dir " + output + "\""
    os.system(command)

# ---------------------------------------------------------------------------------------------------- #

if __name__ == "__main__":
    if(len(sys.argv) == 7):

        user = sys.argv[1]
        emotion = sys.argv[2]
        picture = sys.argv[3]
        all = eval(sys.argv[4])
        show = eval(sys.argv[5])
        originalPath = sys.argv[6]

        print("USER: " + str(user) +
              "  |  EMOTION: " + str(emotion) +
              "  |  PICTURE: " + str(picture) +
              "  |  ALL: " + str(all) +
              "  |  SHOW: " + str(show) + "\n")

        fix = "\\".join(os.path.realpath(__file__).split("\\")[:-1])
        os.chdir(fix)
        cwd = "\\".join(os.getcwd().split("\\")[:-1])

        print("\nCWD: " + cwd + "\n")

        print("Started: extraction")
        extract(picture, all) # Runs OpenFace data extraction
        print("Finished: extraction" + "\n")

        print("Started: grouping")
        group(emotion, picture) # Groups subject's data by emotion
        print("Finished: grouping" + "\n")

        print("Started: clustering")
        clustering(emotion, picture) # Does clustering
        print("Finished: clustering")

        print("Started: turn back")
        turnBack(user, emotion, picture, originalPath) # Returns result to application
        print("Finished: turn back")

        # Shows picture used for clustering
        if show:
            image = img.imread(str(cwd + "\\Images\\" + picture))
            plt.imshow(image)
            plt.show()

    else:
        print("WRONG_ARGS_NUMBER:")
        #for i in range(0, len(sys.argv)):
        #    print("#" + str(i+1) + ":\t\t" + sys.argv[i])
        for arg in sys.argv:
            print(arg)
