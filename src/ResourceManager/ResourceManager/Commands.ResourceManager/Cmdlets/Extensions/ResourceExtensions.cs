﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

namespace Microsoft.Azure.Commands.ResourceManager.Cmdlets.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using Microsoft.Azure.Commands.ResourceManager.Cmdlets.Components;
    using Microsoft.Azure.Commands.ResourceManager.Cmdlets.Entities.Resources;
    using Microsoft.Azure.Common.Authentication;
    using Microsoft.WindowsAzure.Commands.Utilities.Common;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// A helper class that handles common tasks that deal with the <see cref="Resource{JToken}"/> class.
    /// </summary>
    internal static class ResourceExtensions
    {
        /// <summary>
        /// Converts a <see cref="Resource{JToken}"/> object into a <see cref="PSObject"/> object.
        /// </summary>
        /// <param name="resource">The <see cref="Resource{JToken}"/> object.</param>
        /// <param name="objectFormat">The <see cref="ResourceObjectFormat"/></param>
        internal static PSObject ToPsObject(this Resource<JToken> resource, ResourceObjectFormat objectFormat)
        {
            var resourceType = ResourceIdUtility.GetResourceType(resource.Id);
            var extensionResourceType = ResourceIdUtility.GetExtensionResourceType(resource.Id);

            var objectDefinition = new Dictionary<string, object>
            {
                { "Name", resource.Name },
                { "ResourceId", resource.Id },
                { "ResourceName", ResourceIdUtility.GetResourceName(resource.Id) },
                { "ResourceType", resourceType },
                { "ExtensionResourceName", ResourceIdUtility.GetExtensionResourceName(resource.Id) },
                { "ExtensionResourceType", extensionResourceType },
                { "Kind", resource.Kind },
                { "ResourceGroupName", ResourceIdUtility.GetResourceGroupName(resource.Id) },
                { "Location", resource.Location },
                { "SubscriptionId", ResourceIdUtility.GetSubscriptionId(resource.Id) },
                { "Tags", TagsHelper.GetTagsHashtables(resource.Tags) },
                { "Plan", resource.Plan.ToJToken().ToPsObject(objectFormat) },
                { "Properties", ResourceExtensions.GetProperties(resource, objectFormat) },
                { "CreatedTime", resource.CreatedTime },
                { "ChangedTime", resource.ChangedTime },
                { "ETag", resource.ETag },
            };

            var psObject = PowerShellUtilities.ConstructPSObject(
                (resourceType + extensionResourceType).Replace('/', '.'),
                objectDefinition.Where(kvp => kvp.Value != null).SelectManyArray(kvp => new[] { kvp.Key, kvp.Value }));

            psObject.TypeNames.Add(Constants.MicrosoftAzureResource);
            return psObject;
        }

        /// <summary>
        /// Gets the properties object
        /// </summary>
        /// <param name="resource">The <see cref="Resource{JToken}"/> object.</param>
        /// <param name="objectFormat">The <see cref="ResourceObjectFormat"/></param>
        private static object GetProperties(Resource<JToken> resource, ResourceObjectFormat objectFormat)
        {
            if (resource.Properties == null)
            {
                return null;
            }

            return objectFormat == ResourceObjectFormat.Legacy
                ? JsonUtilities.DeserializeJson(resource.Properties.ToString())
                : (object)resource.Properties.ToPsObject(objectFormat);
        }

        /// <summary>
        /// Converts a <see cref="JToken"/> to a <see cref="Resource{JToken}"/>.
        /// </summary>
        /// <param name="jtoken">The <see cref="JToken"/>.</param>
        internal static Resource<JToken> ToResource(this JToken jtoken)
        {
            return jtoken.ToObject<Resource<JToken>>(JsonExtensions.JsonMediaTypeSerializer);
        }

        /// <summary>00
        /// Converts a <see cref="JToken"/> to a <see cref="Resource{JToken}"/>.
        /// </summary>
        /// <typeparam name="TType">The type of the properties.</typeparam>
        /// <param name="jtoken">The <see cref="JToken"/>.</param>
        internal static Resource<TType> ToResource<TType>(this JToken jtoken)
        {
            return jtoken.ToObject<Resource<TType>>(JsonExtensions.JsonMediaTypeSerializer);
        }
    }
}