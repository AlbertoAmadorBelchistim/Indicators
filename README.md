# ATAS Indicators

[Please read the docs here](https://docs.atas.net/)

---

## Possible Build Error

If you encounter the following error during project build:

```
The "GenerateDepsFile" task failed unexpectedly.
System.ArgumentException: An item with the same key has already been added. Key: ATAS.Indicators.Technical
   at System.Collections.Generic.CollectionExtensions.LibraryCollectionToDictionary[T](IReadOnlyList`1 collection)
   ...
```

This may happen due to a temporary conflict in dependency resolution.  
In most cases, it's enough to **wait a few seconds and try building the project again**.

If the problem persists, or you have any other problems please open the GitHub Issue.
