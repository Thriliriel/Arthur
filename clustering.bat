echo F | xcopy "camImage.png" "Clustering\Images\%3" /Y
python Clustering\Scripts\clustering.py %1 %2 %3 %4 %5 "%cd%"