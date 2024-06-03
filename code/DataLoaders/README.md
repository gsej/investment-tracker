DataLoaders are intended to read files in particular formats, perform some checking and
cleaning of data if necessary, and then write the data to the database.

They expect the files to be in particular locations (TODO: not implemented yet).

They differ from the more low level FileReaders in tha the readers perform the actual physical reading of the 
files, transforming the contents into Dtos, which are then converted to Entities by the Loaders.