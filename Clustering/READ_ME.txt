[18:55, 8/7/2020] +55 53 8135-0813: Dentro de "OpenFace" ficam os binários
[18:55, 8/7/2020] +55 53 8135-0813: O projeto é esse: https://github.com/TadasBaltrusaitis/OpenFace
[18:56, 8/7/2020] +55 53 8135-0813: A versão (Windows) que usei é essa: https://github.com/TadasBaltrusaitis/OpenFace/releases/tag/OpenFace_2.2.0
[18:56, 8/7/2020] +55 53 8135-0813: No caso do binário para Windows, tens que rodar um script PowerShell para baixar os modelos
[18:57, 8/7/2020] +55 53 8135-0813: Dentro de Images, vão todas as imagens capturadas pela webcam na execução do clustering
[18:58, 8/7/2020] +55 53 8135-0813: Também tem um diretório chamado "Dataset", as pastas com as imagens que compartilhei no GDrive devem ficar alí dentro
[18:58, 8/7/2020] +55 53 8135-0813: s01, s02, ...
[18:59, 8/7/2020] +55 53 8135-0813: Basicamente são esses diretórios que tens que criar manualmente, os outros são criados na execução
[18:59, 8/7/2020] +55 53 8135-0813: No caso das bibliotecas Python, se lembro bem, vais precisar apenas da "sklearn" e "pandas"
[19:00, 8/7/2020] +55 53 8135-0813: *extra
[19:05, 8/7/2020] +55 53 8135-0813: Para executar é só selecionar a emoção no dropdown e dar "Run"
[19:06, 8/7/2020] +55 53 8135-0813: No geral, é isso

Caso nao funcione de primeira:
Edita aquele clustering.bat e troca %4 por True (demora uns minutos, mas só na primeira vez... depois pode voltar para %4).

Diretorios que tem que ter aqui:
Clusters
CSV
Images (https://drive.google.com/drive/folders/1QAm5mEX5-r1wtf2gI9GHFELoFLFyG7ug)
Logs
OpenFace
OpenFace_Output
Profiles
Scripts