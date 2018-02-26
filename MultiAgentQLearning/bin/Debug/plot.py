import matplotlib.pyplot as plt
import pandas as pd

ylabel="Q difference"
xlabel="Simulation time"
df=pd.read_csv("./output.csv",index_col="STEPS")
print df

ax = df.plot(title="", fontsize=12)
ax.set_xlabel(xlabel)
ax.set_ylabel(ylabel)
plt.show()


