# What is this?

Shamir Secret Sharing Scheme implementation by C#. This library is compatible with https://github.com/codahale/shamir .

# build

```
dotnet build
```

# run test

```
dotnet test
```

# Note on test

For polyglot testing, we use
 - xunit
 - IKVM (DLLs are taken from Ubuntu 18.04 ikvm/bionic 8.1.5717.0+ds-1)
 - DLL file converted from `shamir-0.7.0.jar` using `ikvmc` on Ubuntu 18.04.
