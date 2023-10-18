﻿namespace Neuroglia.Data.Infrastructure.ObjectStorage.Services;

/// <summary>
/// Defines the fundamentals of a storage that manages data as objects
/// </summary>
public interface IObjectStorage
{

    /// <summary>
    /// Creates a new bucket
    /// </summary>
    /// <param name="name">The name of the bucket to create</param>
    /// <param name="tags">A name/value mapping of the bucket's tags, if any</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IBucketDescriptor"/></returns>
    Task<IBucketDescriptor> CreateBucketAsync(string name, IDictionary<string, string>? tags = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether or not the <see cref="IObjectStorage"/> contains the specified bucket
    /// </summary>
    /// <param name="name">The name of the bucket to check</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A boolean indicating whether or not the <see cref="IObjectStorage"/> contains the specified bucket</returns>
    Task<bool> ContainsBucketAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all buckets contained by the <see cref="IObjectStorage"/>
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IAsyncEnumerable{T}"/>, used to asynchronously enumerate <see cref="IBucketDescriptor"/>s</returns>
    IAsyncEnumerable<IBucketDescriptor> ListBucketsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the bucket with the specified name
    /// </summary>
    /// <param name="name">The name of the bucket to get</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IBucketDescriptor"/></returns>
    Task<IBucketDescriptor> GetBucketAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the tags of the specified bucket
    /// </summary>
    /// <param name="name">The name of the bucket to set the tags of</param>
    /// <param name="tags">A name/value mapping of the bucket's tags</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task SetBucketTagsAsync(string name, IDictionary<string, string> tags, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the tags of the specified bucket
    /// </summary>
    /// <param name="name">The name of the bucket to remove tags from</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task RemoveBucketTagsAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the specified bucket
    /// </summary>
    /// <param name="name">The name of the bucket to remove</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task RemoveBucketAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all buckets contained by the specified bucket
    /// </summary>
    /// <param name="bucketName">The name of the bucket to list the objects of</param>
    /// <param name="prefix">The prefix, if any, of the objects to list</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IAsyncEnumerable{T}"/>, used to asynchronously enumerate <see cref="IBucketDescriptor"/>s</returns>
    IAsyncEnumerable<IObjectDescriptor> ListObjectsAsync(string bucketName, string? prefix = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new object into the specified bucket
    /// </summary>
    /// <param name="bucketName">The name of the bucket to add the object to</param>
    /// <param name="name">The object's name</param>
    /// <param name="contentType">The object's content type</param>
    /// <param name="stream">The <see cref="Stream"/> that contains the object's data</param>
    /// <param name="tags">A name/value mapping of the object's tags, if any</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IObjectDescriptor"/></returns>
    Task<IObjectDescriptor> PutObjectAsync(string bucketName, string name, string contentType, Stream stream, IDictionary<string, string>? tags = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the tags of the specified object
    /// </summary>
    /// <param name="bucketName">The name of the bucket the object to tag belongs to</param>
    /// <param name="name">The name of the object to set the tags of</param>
    /// <param name="tags">A name/value mapping of the object's tags</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task SetObjectTagsAsync(string bucketName, string name, IDictionary<string, string> tags, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the tags of the specified object
    /// </summary>
    /// <param name="bucketName">The name of the bucket the object to remove the tags of belongs to</param>
    /// <param name="name">The name of the object to remove tags from</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task RemoveObjectTagsAsync(string bucketName, string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the specified object
    /// </summary>
    /// <param name="bucketName">The name of the bucket the object to remove belongs to</param>
    /// <param name="name">The name of the object to remove</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task RemoveObjectAsync(string bucketName, string name, CancellationToken cancellationToken = default);

}