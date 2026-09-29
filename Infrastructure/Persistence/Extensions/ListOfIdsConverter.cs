// using Microsoft.EntityFrameworkCore.ChangeTracking;
// using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
//
// namespace Infrastructure.Persistance;
//
// // Recieves a ValueObject and converts it to a list of string with ids
// public class ListOfIdsConverter : ValueConverter<IReadOnlyList<UserId>, string>
// {
//     public ListOfIdsConverter(ConverterMappingHints? mappingHints = null)
//         : base(
//             v => string.Join(',', v.Select(x => x.Value)), // Sending as string to provider
//             v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
//                 .Select(s => UserId.Create(Guid.Parse(s))).ToList(), // Formatting as UserId from provider
//             mappingHints)
//     {
//     }
// }
//
// public class ListOfIdsComparer : ValueComparer<IReadOnlyList<UserId>>
// {
//     public ListOfIdsComparer() : base(
//         (t1, t2) => t1!.SequenceEqual(t2!),
//         t => t.Select(x => x!.GetHashCode()).Aggregate((x, y) => x ^ y),
//         t => t)
//     {
//     }
// }