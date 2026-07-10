import numpy as np
import pyarrow as pa
import pyarrow.parquet as pq

NUM_COLUMNS = 3
NUM_ROWS = 10
ROW_GROUP_SIZE = 50
OUTPUT_PATH = "random_walk.parquet"


def generate_random_walk_columns(num_columns, num_rows, seed = 42):
    rng = np.random.default_rng(seed)
    columns = {}
    for i in range(num_columns):
        start = rng.uniform(1000.0, 2000.0)
        steps = rng.uniform(-1.0, 1.0, size=num_rows - 1)
        series = np.empty(num_rows, dtype=np.float64)
        series[0] = start
        series[1:] = start + np.cumsum(steps)
        columns[f"col_{i:02d}"] = series
    return columns


def main() -> None:
    data = generate_random_walk_columns(NUM_COLUMNS, NUM_ROWS)
    table = pa.table(data)

    with pq.ParquetWriter(OUTPUT_PATH, table.schema) as writer:
        for offset in range(0, NUM_ROWS, ROW_GROUP_SIZE):
            writer.write_table(table.slice(offset, ROW_GROUP_SIZE))

    parquet_file = pq.ParquetFile(OUTPUT_PATH)
    print(f"Wrote {OUTPUT_PATH}")
    print(f"  rows:       {parquet_file.metadata.num_rows}")
    print(f"  columns:    {parquet_file.metadata.num_columns}")
    print(f"  row groups: {parquet_file.metadata.num_row_groups}")


if __name__ == "__main__":
    main()
