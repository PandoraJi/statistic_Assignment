# Assignment: Statistics calculating HTTP server

## Objective

Create a service with one endpoint that:

1. accepts HTTP requests
2. expects parquet data to be contained in these requests
3. performs calculations on the parquet data in these requests
4. returns the results of the calculations as a new parquet file

Let's call this endpoint `/statistics`.

### /statistics

#### Request

The input parquet files are expected to contain floating-point data columns.

Example:

| C1  | C2   | C3  |
| --- | ---- | --- |
| 2.5 | -8.0 | 0.0 |
...

#### Response

The resulting parquet file should contain 5 columns: min, max, p10, p50 and p90.
Each row represents the five statistics for the same numbered row in the input.

Example (based on the request above):

| min  | max | p10  | p50 | p90 |
| ---- | --- | ---- | --- | --- |
| -8.0 | 2.5 | -6.4 | 0.0 | 2.0 |
...

where pN denotes the Nth percentile.

## Generating parquet files

A [python script](./generate_parquet.py) is provided which can generate a random parquet file.

Feel free to modify it.

## Time budget

We expect this task to take no more than 4 hours.
The scope of the implementation is fairly small, and not expected to be ready for deployment with dockerfiles or the like.

## Notes

You're allowed to use any language, pick the one that best demonstrates your knowledge!

You can also use any framework or library you'd like in the solution. Feel free to use AI.

If you put the solution on Github, do so in a private repository and share it with [telefragged](https://github.com/telefragged) (vbr@resoptima.com)
and [aryelfnd](https://github.com/aryelfnd) (afe@resoptima.com) on GitHub.

Alternatively, create a zip or tar file and send it to us via email.
