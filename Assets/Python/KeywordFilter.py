import os
from nltk.corpus import stopwords  
from nltk.tokenize import word_tokenize  
from nltk.stem import WordNetLemmatizer

class LeafNode: #map keywords occurences and siblins nodes that use each
    map_kw = {} #keyword -> list of tuples (utterance_id found the keyword, occurences at these utt)

    def insert(self, dialog_id, kw, occurence):
        if not kw in self.map_kw:
            self.map_kw[kw] = []
        self.map_kw[kw].append((dialog_id, occurence))
    
    def get_data(self, kw):
        if not kw in self.map_kw:
            return None
        
        counter = 0
        siblins = []

        for t in self.map_kw[kw]:
            counter += t[1]
            siblins.append(t[0])
            
        return (counter, siblins)

class KeywordFilter:
    globalPath = ""

    def __init__(self, newPath = ""):
        self.globalPath = newPath

    def OutputTree(self, data_structure, deep):
        if not isinstance(data_structure,dict):
            print(data_structure)
            return
        print("")
        for key in data_structure:
            for i in range(deep):
                print(" ", end = '')
            print(key + ": ", end = '')
            self.OutputTree(data_structure[key], deep+1)

    #data structure that merge all words at the same dialog tree level and sum its occurences
    #build it only if there is keywords on keywords.txt available
    def buildRFTree(self, dic_utt):
        #root      -> topics_id{}
        #topics    -> dialogs_id{}
        #dialogs_id -> tree_lvl{} (from dialog tree)
        #tree_lvl -> word{}
        #word -> occurences
    
        root = {}

        #topic = {dialogs}
        #dialogs = {dialogs split by level}
        #dialog_lvl = {all keywords for each level}
        #keywords = {occurences}
    
        for utt_id in dic_utt:
            #utt_id ADScs0302 -> ADS: topics_id | cs -> dialog_id | 03 -> dialog_number | 02 -> parent_number
            topic_id = utt_id[0:3]
            dialog_id = utt_id[3:5] 
            parent_id = utt_id[7:]
        
            #topic not found
            if not topic_id in root:
                root[topic_id] = {}

            #dialogue not found (utterance)
            if not dialog_id in root[topic_id]:
                (root[topic_id])[dialog_id] = {}
        
            #tree_lvl not found
            if not parent_id in (root[topic_id])[dialog_id]:
                ( (root[topic_id]) [dialog_id] )[parent_id] = LeafNode()

            #insert/sum keywords counter
            dic_keywords = dic_utt[utt_id]
            for keyword in dic_keywords:
                weight, occ = dic_keywords[keyword]
                occurence = int(occ)
                ((root[topic_id])[dialog_id])[parent_id].insert(utt_id, keyword, occurence)
    
        #self.OutputTree(root,0)
        return root
    
    #same key
    def frequency(self, new_keywords, dic_utt, rf_tree):
        #basically each utterance has a group of keywords with weight and a counter of its occurences
    
        #dic_utt  { utt_id : dic_keys }
        #dic_keys { word : (weight, occurences)}

        ## --> etapa de busca pelos dados 
        # STEP1: lista ids das novas palavras-chave
    
        track_simple_occurences = {} #for each <utt_id> count total occurences of keywords (this is the divisor part of frequency)
    
        for utt_id in new_keywords:
        
            if utt_id not in dic_utt:
                dic_utt[utt_id] = {}
        
            ## getting the necessary information ##
            vec_new_keywords = new_keywords[utt_id]
        
            # STEP2: para cada ids das palavras-chaves separar topic_id, dialog_id, tree_lvl
            topic_id = utt_id[0:3]
            dialog_id = utt_id[3:5]
            parent_id = utt_id[7:]
            siblins = {}

            if topic_id in rf_tree and dialog_id in rf_tree[topic_id] and parent_id in rf_tree[topic_id][dialog_id]:    
                # STEP4: para cada palavra-chave buscar siblins
                siblins = rf_tree[topic_id][dialog_id][parent_id] # dic{key: keyword | value -> (total_occurences, list of utt_id that use it) }  
            
        
                # STEP5: update siblins
                for kw in vec_new_keywords:
                    data = siblins.get_data(kw)
                    print(data)

                    if data == None:
                        continue

                    total_siblins = data[0] + 1
                    siblins_ids = data[1]
                    #find the keyword to be updated from siblin 
                    for s_id in siblins_ids:
                        siblin_kw = dic_utt[s_id]
                        count_s_kw = 0

                        # ** needs to find out simple occurences (keywords occurence on utt_id) / (all keywords occurences on utt_id)
                        if s_id not in track_simple_occurences:
                            track_simple_occurences[s_id] = 0                
                            # count total keywords at the siblin
                            for s_kw_index in siblin_kw:
                                track_simple_occurences[s_id] += int(siblin_kw[s_kw_index][1]) # * remember: keyword -> tuple (weight, occurences)

                        kw_smp_occ = int(siblin_kw[kw][1])

                        total_simples = track_simple_occurences[s_id]

                        simple_frequency = kw_smp_occ / float(total_simples)

                        siblin_frequency = kw_smp_occ / float(total_siblins) 

                        weight = (simple_frequency + siblin_frequency) / float(2)


                        (dic_utt[s_id])[kw] = (weight, kw_smp_occ)

        
            # STEP6: buscar todas palavras-chave pertencentes ao utt_id da nova palavra_chave
            dialog_keywords = dic_utt[utt_id]

            # STEP7: update keywords from same utt_id
            #verify if needs to count simple occurences
            if utt_id  not in track_simple_occurences:
            
                track_simple_occurences[utt_id] = 0
            
                for kw_index in dialog_keywords:
                    track_simple_occurences[utt_id] += int(dialog_keywords[kw_index][1])
        
            total_simples = track_simple_occurences[utt_id] + len(vec_new_keywords)
        

            #update keywords - same utt_ID *total_simples changed 
            for kw in dialog_keywords:
                if kw in vec_new_keywords:
                    continue
        
                weight = 0
                kw_smp_occ = int(dialog_keywords[kw][1])
                simple_frequency =  kw_smp_occ / float(total_simples)
        
                try:
                    total_siblins = siblins.get_data(kw)[0]
                    siblin_frequency = kw_smp_occ / total_siblins
                    weight = (simple_frequency + siblin_frequency) / float(2)
            
                except:
            
                    weight = (simple_frequency + 1) / float(2)
    
                (dic_utt[utt_id])[kw] = (weight, kw_smp_occ)

            # STEP8: update/insert NEW keywords
            for kw in vec_new_keywords:
                kw_smp_occ = 1
            
                if kw in dic_utt[utt_id]:
                    kw_smp_occ += int(dialog_keywords[kw][1])
            
                weight = 0
                simple_frequency = kw_smp_occ / float(total_simples)
        
                try:
            
                    data = siblins.get_data(kw)
                    total_siblins = data[0] + 1                
                    siblin_frequency = kw_smp_occ / total_siblins
                    weight = (simple_frequency + siblin_frequency) / float(2)
            
                except:
            
                    weight = (simple_frequency + 1) / float(2)

                dic_utt[utt_id][kw] = (weight, kw_smp_occ)

        del track_simple_occurences

    #update frequency
    def updateHistoric(self):
        ################# LOADING/MAPPING ALL KEYWORDS #################

        f = open(self.globalPath + "keywords.txt", "r")
        dic_utt = {}   #dic_utt  { utt_id : dic_keys }
                       #dic_keys { word : (weight, occurences, tree_lvl)}
        for i in f:
            vec = i.rstrip('\n').split(" ")
    
            if len(vec) < 4:
                break 

            utt_id = vec[0]
            w = vec[1]
            weight = vec[2]
            occ = vec[3]

            if not utt_id in dic_utt:
                new_dic_keys = {}
                dic_utt[utt_id] = new_dic_keys
    
            (dic_utt[utt_id])[w] = (weight, occ)
        f.close()

        if os.path.exists(self.globalPath + 'keywordsBACKUP.txt'):
            os.remove(self.globalPath + 'keywordsBACKUP.txt')

        os.rename(self.globalPath + 'keywords.txt', self.globalPath + 'keywordsBACKUP.txt')    

        ############ PROCESSING NEW KEYWODS FROM HISTORIC ################


        #load new inputs from Arthur
        f = open(self.globalPath + "AutobiographicalStorage/historic.txt", "r")
        historic = {}
        for x in f:
            vec = x.rstrip('\n').split(";")
            historic[vec[2]] = vec[1] # x[0] = dialog_id father | x[1] = answer | x[2] = choosen dialog_id 
    
            #historic[x[0:9]] = x[9:]

        f.close()

        #update keywords
        stop_words = set(stopwords.words('english'))  
        lemmatizer = WordNetLemmatizer()
        new_keywords = {}


        #f.write("<ut_id> <word> <weight> <total_occurences>\n")       
        for k in historic:
            word_tokens = word_tokenize(historic[k])  

            filtered_sentence = [w.lower() for w in word_tokens if not w in stop_words if w.isalnum()]  
            lemmatized_vec = [lemmatizer.lemmatize(w) for w in filtered_sentence]
            new_keywords[k] = lemmatized_vec 
    
            #print(word_tokens)  
            #print(filtered_sentence)
            #print(lemmatized_sentence)  
            #for e in lemmatized_vec:
            #    f.write(k + " " + e + " " + '0.1' + " " + '1' '\n')
        

        ################ CREATE/UPDATE KEYWORDS ###############

        rftree = self.buildRFTree(dic_utt)
        self.frequency(new_keywords, dic_utt, rftree)
        #relativeFreq(rf_tree, dic_utt)
        #print(dic_utt)


        f = open(self.globalPath + "keywords.txt", "a")
        for i in dic_utt:
            dk = dic_utt[i]
    
            for j in dk:
                x, y = dk[j]
                foo = float(x)
                f.write(("{0} {1} {2:.3f} {3}\n").format(i,j,foo,y))

        f.close()