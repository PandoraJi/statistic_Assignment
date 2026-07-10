import pyarrow as pa
import pyarrow.parquet as pq
NUM_COLUMNS = 3
NUM_ROWS = 2
ROW_GROUP_SIZE = 5
OUTPUT_PATH = "statisticsample.parquet"

def createfile()-> None:
    dataitem = {
        "C1": [2.5],
        "C2": [-8.0],
        "C3": [0.0]
    }

    table = pa.table(dataitem)

    pq.write_table(table, OUTPUT_PATH)

    parquet_file = pq.ParquetFile(OUTPUT_PATH)

    print(f"Created {OUTPUT_PATH}")
    print(f"Rows: {parquet_file.metadata.num_rows}")
    print(f"Columns: {parquet_file.metadata.num_columns}")
    print(f"Row Groups: {parquet_file.metadata.num_row_groups}")

if __name__ == "__main__":
    createfile()