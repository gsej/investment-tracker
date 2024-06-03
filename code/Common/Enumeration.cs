//
// // reconstruct from https://lostechies.com/jimmybogard/2008/08/12/enumeration-classes/
// // with help from https://eliot-jones.com/2015/03/entity-framework-enum
//
//
// The mapping to entity framework is done like:
//
// modelBuilder.Entity<MyEntity>
//     .Property(e => e.Status)
//     .HasConversion(type => type.Value, typeValue => Enumeration.FromValue<Statuses>(typeValue)