# Use Problem Details to give HTTP clients actionable errors

A shopper asks for three keyboards. The product exists, but the shop has two. The client needs to distinguish the stock conflict from a missing product and invalid input to offer the right next action.

## Design the behavior

Return `400` and field-level `errors` for an invalid request. Return `404` for an unknown SKU and `409` with `availableQuantity` for a stock conflict. Use a stable `type` such as `urn:pathbit:problem:insufficient-stock` so clients can decide without reading localized text.

## Run the lab

Run the preview with `keyboard` and quantity `2`, then `3`, and call `/operations/fail`. Compare `200`, `409`, and `500`. The tests also exercise field errors, malformed JSON, and unknown routes.

[Full guide](https://github.com/pathbit/pathbit-academy-dotnet/tree/master/0004_problem_details/article/ARTICLE.en.md)

Developed with ❤️ by [Pathbit](https://www.pathbit.com.br/)
