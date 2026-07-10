import pyarrow.parquet as pq

table = pq.read_table("random_walk.parquet")

print(table)
print(table.to_pandas().head())