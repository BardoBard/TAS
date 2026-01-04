using System;
using Thor.Core;

namespace Lithium.Core.Thor.Core
{
    public interface ITasReflectionService : ITasService
    {
        /// <summary>
        /// Gets the value of a field on the given object by name.
        /// </summary>
        /// <param name="obj"> The object to get the field from. </param>
        /// <param name="fieldName"> The name of the field to get. </param>
        /// <param name="value"> The value of the field. </param>
        /// <typeparam name="T"> The type of the field. </typeparam>
        /// <returns> True if the field was found and the value was retrieved, false otherwise. </returns>
        bool GetFieldValue<T>(object obj, string fieldName, out T value);
        
        /// <summary>
        /// Sets the value of a field on the given object by name.
        /// </summary>
        /// <param name="obj"> The object to set the field on. </param>
        /// <param name="fieldName"> The name of the field to set. </param>
        /// <param name="value"> The value to set the field to. </param>
        /// <typeparam name="T"> The type of the field. </typeparam>
        /// <returns> True if the field was found and set, false otherwise. </returns>
        bool SetFieldValue<T>(object obj, string fieldName, T value);
        
        /// <summary>
        /// Gets a delegate to a function on the given object by name.
        /// </summary>
        /// <param name="obj"> The object to get the function from. </param>
        /// <param name="functionName"> The name of the function to get. </param>
        /// <param name="functionDelegate"> The delegate to the function. </param>
        /// <typeparam name="T"> The type of the delegate. </typeparam>
        /// <returns> True if the function was found and the delegate was created, false otherwise. </returns>
        /// <remarks> T must be a delegate type matching the signature of the function. </remarks>
        bool GetFunctionDelegate<T>(object obj, string functionName, out T functionDelegate) where T : Delegate;
        
        /// <summary>
        /// Gets a delegate to a static function on the given type by name.
        /// </summary>
        /// <param name="type"> The type to get the function from. </param>
        /// <param name="functionName"> The name of the function to get. </param>
        /// <param name="functionDelegate"> The delegate to the function. </param>
        /// <typeparam name="T"> The type of the delegate. </typeparam>
        /// <returns> True if the function was found and the delegate was created, false otherwise. </returns>
        bool GetFunctionDelegate<T>(Type type, string functionName, out T functionDelegate) where T : Delegate;
        
        /// <summary>
        /// Copies field values from one object to another.
        /// </summary>
        /// <param name="source"> The source object. </param>
        /// <param name="fieldName"> The name of the field to copy. </param>
        /// <param name="destination"> The destination object. </param>
        /// <param name="destinationFieldName"> The name of the field to copy to. </param>
        /// <returns> True if the field values were copied successfully, false otherwise. </returns>
        bool CopyFieldValues(object source, string fieldName, object destination, string destinationFieldName);

        /// <summary>
        /// Performs a deep copy of an asset/prefab reference.
        /// </summary>
        /// <param name="source"> The source asset/prefab reference. </param>
        /// <param name="destination"> The destination asset/prefab reference. </param>
        /// <typeparam name="T"> The type of the asset/prefab reference. </typeparam>
        /// <returns> True if the deep copy was successful, false otherwise. </returns>
        bool DeepCopyAssetReference<T>(T source, out T destination) where T : IAssetReference;
    }
}