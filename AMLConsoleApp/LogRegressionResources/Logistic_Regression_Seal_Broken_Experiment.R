#----------------------------------------------------------------------
#Step 1: Load libraries to use,

#install.packages("dplyr")
#install.packages("caTools")
#install.packages("ElemStatLearn")

library(dplyr)
library(caTools)
library(ElemStatLearn)
library(ggplot2)
library(corrplot)

#----------------------------------------------------------------------
#Step 2: Read in IOT Fridge data export from external CSV. 

#Read data into iotdata object from CSV.
iotdata <- read.csv("FInal_Seal_Broken_Data.csv")

#Get some summary stats on the iotdata object.
head(iotdata)
nrow(iotdata)
tail(iotdata)
summary(iotdata)

#----------------------------------------------------------------------

#----------------------------------------------------------------------
#Step 3: Trim data from core sample data and experiment with some common ggplot charts. 

sealBrokenData <- subset(iotdata, DeviceId < 33821)

#Alternate write function to save the state of the data. 
#write.csv(sealBrokenData, "IOT_Fridge_Data_V1.csv")

#Write out a plot chart to view linear coefficients. 
sealData = cor(sealBrokenData[,1:8])
corrplot.mixed(sealData)

#----------------------------------------------------------------------
#Step 4: Create the training and test set.

isSealBroken <- subset(sealBrokenData)[, c("DeviceId", "InsideTemperature", "SealThreshold", "SealBroken")]

summary(isSealBroken)

isSealBroken = isSealBroken[, 2:4]
set.seed(123)
split = sample.split(isSealBroken$SealBroken, SplitRatio = 0.75)
trainingSet = subset(isSealBroken, split == TRUE)
testSet = subset(isSealBroken, split == FALSE)

summary(trainingSet)
summary(testSet)

#----------------------------------------------------------------------
#Step 5: Feature Scaling

#trainingSet[, 1:2] = scale(trainingSet[, 1:2])
#testSet[, 1:2] = scale(testSet[, 1:2])


# Fitting Logistic Regression to the Training set
classifier = glm(formula = SealBroken ~ .,
                 family = binomial,
                 data = testSet)

# Predicting the Test set results
probPred = predict(classifier, type = 'response', newdata = testSet[-3])
yPred = ifelse(probPred > 0.5, 1, 0)

# Making the Confusion Matrix
cm = table(testSet[, 3], yPred > 0.5)

#----------------------------------------------------------------------
# Step 6: Visualising the Training set results.

#Visualizing the Training set results.

#Note: Need to still fix some errors here. 

library(ElemStatLearn)

set = trainingSet

X1 = seq(min(set[, 1]) - 1, max(set[, 1]) + 1, by = 1)
X2 = seq(min(set[, 2]) - 1, max(set[, 2]) + 1, by = 1)

grid_set = expand.grid(X1, X2)
colnames(grid_set) = c('Moisture', 'InsideTemperature')
prob_set = predict(classifier, type = 'response', newdata = grid_set)
y_grid = ifelse(prob_set > 0.5, 1, 0)
plot(set[, -3],
     main = 'Logistic Regression (Training set)',
     xlab = 'Moisture', ylab = 'InsideTemperature',
     xlim = range(X1), ylim = range(X2))
contour(X1, X2, matrix(as.numeric(y_grid), length(X1), length(X2)), add = TRUE)
points(grid_set, pch = '.', col = ifelse(y_grid == 1, 'springgreen3', 'tomato'))
points(set, pch = 21, bg = ifelse(set[, 3] == 1, 'green4', 'red3'))

# Visualising the Test set results
library(ElemStatLearn)
set = testSet

X1 = seq(min(set[, 1]) - 1, max(set[, 1]) + 1, by = 0.01)
X2 = seq(min(set[, 2]) - 1, max(set[, 2]) + 1, by = 0.01)
grid_set = expand.grid(X1, X2)
colnames(grid_set) = c('Moisture', 'InsideTemperature')
prob_set = predict(classifier, type = 'response', newdata = grid_set)
y_grid = ifelse(prob_set > 0.5, 1, 0)
plot(set[, -3],
     main = 'Logistic Regression (Test set)',
     xlab = 'Moisture', ylab = 'InsideTemperature',
     xlim = range(X1), ylim = range(X2))
contour(X1, X2, matrix(as.numeric(y_grid), length(X1), length(X2)), add = TRUE)
points(grid_set, pch = '.', col = ifelse(y_grid == 1, 'springgreen3', 'tomato'))
points(set, pch = 21, bg = ifelse(set[, 3] == 1, 'green4', 'red3'))


